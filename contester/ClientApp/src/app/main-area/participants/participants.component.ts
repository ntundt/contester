import {Component, OnInit} from '@angular/core';
import {ContestApplicationsService, ContestService, PrincipalDto} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {FormsModule} from "@angular/forms";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import { TranslateModule } from '@ngx-translate/core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import {tap} from "rxjs/operators";
import {PrincipalCard} from "../../shared/principal-card/principal-card";
import {PrincipalSelectionModal} from "../../shared/principal-selection-modal/principal-selection-modal";

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

  private getParticipants(contestId: string): void {
    this.contestService.apiContestsContestIdParticipantsGet(contestId).subscribe(participants => {
      this.participants = participants.contestParticipants ?? [];
    });
  }

  public ngOnInit(): void {
    this.activatedRoute.parent?.params.subscribe(params => {
      this.contestId = params['contestId'];
      this.getParticipants(this.contestId);
    });
  }

  public addParticipant(): void {
    this.modalService.open(PrincipalSelectionModal).result
      .then((principal: PrincipalDto) => {
        if (principal.type === 'User') {
          this.contestService.apiContestsContestIdParticipantsPost(this.contestId, { participantId: principal.id }).subscribe({
            next: () => {
              this.getParticipants(this.contestId);
            },
          });
        } else if (principal.type === 'Group') {
          this.contestService.apiContestsContestIdParticipantGroupsGroupIdPost(this.contestId, principal.id!).subscribe({
            next: () => {
              this.getParticipants(this.contestId);
            }
          })
        }
      });
  }

  public deleteParticipant(participant: PrincipalDto): void {
    this.contestService.apiContestsContestIdParticipantsUserIdDelete(this.contestId, participant.id ?? '').subscribe(() => {
      this.getParticipants(this.contestId);
    });
  }

  public approveApplication(participant: PrincipalDto): void {
    this.contestApplicationService.apiContestApplicationsApprovePut(this.contestId, participant.id)
      .subscribe(() => {
        this.getParticipants(this.contestId);
      });
  }

  protected readonly faPlus = faPlus;
}
