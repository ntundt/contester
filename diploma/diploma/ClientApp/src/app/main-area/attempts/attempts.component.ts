import { Component } from '@angular/core';
import {AttemptDto, AttemptService, AttemptStatus} from "../../../generated/client";
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
export class AttemptsComponent {
  public attempts: Array<AttemptDto> = [];
  public constructor(private attemptService: AttemptService,
                     activatedRoute: ActivatedRoute,
                     private modalService: NgbModal,
                     private claimsService: ClaimsService) {
    const contestId = activatedRoute.snapshot.params.contestId;
    this.attemptService.apiAttemptsGet(undefined, '-CreatedAt', undefined, undefined, contestId)
      .subscribe(attempts => {
      this.attempts = attempts.attempts ?? [];
    });
  }

  public viewAttemptIfHasClaim(attemptId: string) {
    if (!this.claimsService.hasClaim('ManageAttempts')) return;

    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = attemptId;
  }


  protected readonly AttemptStatus = AttemptStatus;
}
