import {Component, OnInit} from '@angular/core';
import {ContestApplicationsService, ContestService, PrincipalDto} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {FormsModule} from "@angular/forms";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import { TranslateModule } from '@ngx-translate/core';
import {faInbox, faPlus, faWind} from '@fortawesome/free-solid-svg-icons';
import {catchError, tap} from "rxjs/operators";
import {PrincipalCard} from "../../shared/principal-card/principal-card";
import {PrincipalSelectionModal} from "../../shared/principal-selection-modal/principal-selection-modal";
import {of} from "rxjs";

@Component({
  selector: 'app-participants',
  standalone: true,
  imports: [
    FaIconComponent,
    FormsModule,
    TranslateModule,
    PrincipalCard,
  ],
  templateUrl: './participants.component.html',
  styleUrl: './participants.component.css'
})
export class ParticipantsComponent implements OnInit {
  public contestApplications: Array<PrincipalDto> = [];
  public participants: Array<PrincipalDto> = [];
  private contestId: string = '';

  loadingParticipants: boolean = false;

  selectedTab: 'participants' | 'applications' = 'participants';

  public constructor(
    private contestApplicationService: ContestApplicationsService,
    private contestService: ContestService,
    private activatedRoute: ActivatedRoute,
    private modalService: NgbModal,
  ) { }

  private refreshContestApplications() {
    this.contestService.apiContestsContestIdApplicationsGet(this.contestId).pipe(
      tap(contestApplications => this.contestApplications = contestApplications)
    ).subscribe();
  }

  private refreshParticipants(contestId: string): void {
    this.loadingParticipants = true;
    this.contestService.apiContestsContestIdParticipantsGet(contestId)
      .pipe(
        tap(participants => {
          this.participants = participants.contestParticipants ?? [];
          this.loadingParticipants = false;
        }),
        catchError(() => of(this.loadingParticipants = false))
      ).subscribe();
  }

  public ngOnInit(): void {
    this.activatedRoute.parent?.params.subscribe(params => {
      this.contestId = params['contestId'];
      this.refreshParticipants(this.contestId);
      this.refreshContestApplications();
    });
  }

  public addParticipant(): void {
    this.modalService.open(PrincipalSelectionModal).result
      .then((principal: PrincipalDto) => {
        if (principal.type === 'User') {
          this.contestService.apiContestsContestIdParticipantsPost(this.contestId, { participantId: principal.id }).subscribe({
            next: () => {
              this.refreshParticipants(this.contestId);
            },
          });
        } else if (principal.type === 'Group') {
          this.contestService.apiContestsContestIdParticipantGroupsGroupIdPost(this.contestId, principal.id!).subscribe({
            next: () => {
              this.refreshParticipants(this.contestId);
            }
          })
        }
      });
  }

  public deleteParticipant(participant: PrincipalDto): void {
    if (participant.type === 'User') {
      this.contestService.apiContestsContestIdParticipantsUserIdDelete(this.contestId, participant.id ?? '').subscribe(() => {
        this.refreshParticipants(this.contestId);
        this.refreshContestApplications();
      });
    } else if (participant.type === 'Group') {
      this.contestService.apiContestsContestIdParticipantGroupsGroupIdDelete(this.contestId, participant.id ?? '').subscribe(() => {
        this.refreshParticipants(this.contestId);
      });
    }
  }

  public approveApplication(participant: PrincipalDto): void {
    this.contestApplicationService.apiContestApplicationsApprovePut(this.contestId, participant.id)
      .subscribe(() => {
        this.refreshParticipants(this.contestId);
        this.refreshContestApplications();
      });
  }

  protected readonly faPlus = faPlus;
  protected readonly faWind = faWind;
  protected readonly faInbox = faInbox;
}
