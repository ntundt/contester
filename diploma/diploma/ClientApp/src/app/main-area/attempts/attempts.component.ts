import { Component, OnInit } from '@angular/core';
import {AttemptDto, AttemptService, AttemptStatus, UserService} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {DatePipe, NgForOf, NgIf} from "@angular/common";
import {NgbModal, NgbPopover} from "@ng-bootstrap/ng-bootstrap";
import {AttemptSrcViewModalComponent} from "../../shared/attempt-src-view-modal/attempt-src-view-modal.component";
import {ClaimsService} from "../../../authorization/claims.service";

@Component({
  selector: 'app-attempts',
  standalone: true,
  imports: [
    NgForOf,
    DatePipe,
    NgIf,
    NgbPopover
  ],
  templateUrl: './attempts.component.html',
  styleUrl: './attempts.component.css'
})
export class AttemptsComponent implements OnInit {
  public attempts: Array<AttemptDto> = [];

  private currentUserId: string | undefined;

  public constructor(
    private attemptService: AttemptService,
    private activatedRoute: ActivatedRoute,
    private modalService: NgbModal,
    private claimsService: ClaimsService,
    private userService: UserService
  ) { }

  public ngOnInit(): void {
    const contestId = this.activatedRoute.snapshot.params.contestId;
    this.attemptService.apiAttemptsGet(undefined, '-CreatedAt', undefined, undefined, contestId)
      .subscribe(attempts => {
      this.attempts = attempts.attempts ?? [];
    });
    this.userService.apiUsersGet().subscribe(user => {
      this.currentUserId = user.id;
    });
  }

  public viewAttemptIfHasClaim(attemptId: string) {
    if (!this.claimsService.hasClaim('ManageAttempts') 
      && this.currentUserId !== this.attempts.find(a => a.id === attemptId)?.authorId) {
      return;
    }

    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = attemptId;
    modalRef.componentInstance.contestId = this.activatedRoute.snapshot.params.contestId;
  }


  protected readonly AttemptStatus = AttemptStatus;
}
