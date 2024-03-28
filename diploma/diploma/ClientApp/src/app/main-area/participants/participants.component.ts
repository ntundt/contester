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

@Component({
  selector: 'app-participants',
  standalone: true,
  imports: [
    FaIconComponent,
    NgForOf,
    FormsModule,
    NgIf,
  ],
  templateUrl: './participants.component.html',
  styleUrl: './participants.component.css'
})
export class ParticipantsComponent implements OnInit {
  public participants: Array<ContestParticipantDto> = [];
  public participantToAddEmail: string = '';

  private contestId: string = '';

  public constructor(
    private authenticationService: AuthenticationService,
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

  private confirmInvite(): void {
    const modalRef = this.modalService.open(ActionConfirmationModalComponent);
    modalRef.componentInstance.title = 'User not found';
    modalRef.componentInstance.message = 'User with this email is not found. Do you want to invite them?';
    modalRef.componentInstance.actionName = 'Invite';

    modalRef.result.then((result) => {
      if (result) {
        this.authenticationService.apiAuthBeginInvoluntarySignUpPost({email: this.participantToAddEmail}).subscribe(() => {
          this.addParticipant();
        });
      }
    });
  }

  public addParticipant(): void {
    this.contestService.apiContestsContestIdParticipantsPost(this.contestId, { participantEmail: this.participantToAddEmail }).subscribe({
      next: (res) => {
        this.getParticipants(this.contestId);
        this.participantToAddEmail = '';
      },
      error: (err) => {
        if (err.error.err === 102) { // user not found
          this.confirmInvite();
        } else if (err.error.err === 110) {
          alert('User already added');
        }
      }
    });
  }

  public deleteParticipant(participant: ContestParticipantDto): void {
    this.contestService.apiContestsContestIdParticipantsUserIdDelete(this.contestId, participant.id ?? '').subscribe(() => {
      this.participants = this.participants.filter(p => p.id !== participant.id);
    });
  }

  public approveApplication(participant: ContestParticipantDto): void {
    this.contestApplicationService.apiContestApplicationsIdApprovePut(participant.applicationId ?? '').subscribe(() => {
      this.getParticipants(this.contestId);
    });
  }
}
