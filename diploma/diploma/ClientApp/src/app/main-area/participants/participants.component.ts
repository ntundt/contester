import {Component, OnInit} from '@angular/core';
import {AuthenticationService, ContestApplicationsService, ContestParticipantDto, ContestService, UserDto} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgForOf, NgIf} from "@angular/common";
import {FormsModule} from "@angular/forms";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {
  ActionConfirmationModalComponent
} from "../../shared/action-confirmation-modal/action-confirmation-modal.component";
import { TranslateModule } from '@ngx-translate/core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { UserSelectionModalComponent } from '../settings/user-selection-modal/user-selection-modal.component';

@Component({
  selector: 'app-participants',
  standalone: true,
  imports: [
    FaIconComponent,
    NgForOf,
    FormsModule,
    NgIf,
    TranslateModule,
  ],
  templateUrl: './participants.component.html',
  styleUrl: './participants.component.css'
})
export class ParticipantsComponent implements OnInit {
  public participants: Array<ContestParticipantDto> = [];
  private contestId: string = '';

  public constructor(
    private contestApplicationService: ContestApplicationsService,
    private contestService: ContestService,
    private activatedRoute: ActivatedRoute,
    private modalService: NgbModal,
  ) { }


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
    this.modalService.open(UserSelectionModalComponent).result.then((user: UserDto) => {
      this.contestService.apiContestsContestIdParticipantsPost(this.contestId, { participantId: user.id }).subscribe({
        next: () => {
          this.getParticipants(this.contestId);
        },
      });
    });
  }

  public deleteParticipant(participant: ContestParticipantDto): void {
    this.contestService.apiContestsContestIdParticipantsUserIdDelete(this.contestId, participant.id ?? '').subscribe(() => {
      this.getParticipants(this.contestId);
    });
  }

  public approveApplication(participant: ContestParticipantDto): void {
    this.contestApplicationService.apiContestApplicationsIdApprovePut(participant.applicationId ?? '').subscribe(() => {
      this.getParticipants(this.contestId);
    });
  }

  protected readonly faPlus = faPlus;
}
