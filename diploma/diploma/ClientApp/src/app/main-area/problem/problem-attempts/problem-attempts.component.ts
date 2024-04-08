import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgbModal, NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { AttemptDto, AttemptService, UserService } from 'src/generated/client';
import { Constants } from 'src/constants';
import { AttemptSrcViewModalComponent } from 'src/app/shared/attempt-src-view-modal/attempt-src-view-modal.component';
import { PermissionsService } from 'src/authorization/permissions.service';
import { Observable, forkJoin, tap } from 'rxjs';

@Component({
  selector: 'app-problem-attempts',
  standalone: true,
  imports: [
    NgIf,
    NgbPopover,
    DatePipe,
    NgFor,
  ],
  templateUrl: './problem-attempts.component.html',
  styleUrl: './problem-attempts.component.css'
})
export class ProblemAttemptsComponent implements OnInit {

  attempts: AttemptDto[] = [];

  private currentUserId: string | undefined;
  private canViewAnyAttemptSrc: boolean = false;
  @Input() problemId: string | undefined;
  @Output() refresh: EventEmitter<() => void> = new EventEmitter();

  constructor(
    private attemptService: AttemptService,
    private activatedRoute: ActivatedRoute,
    private modalService: NgbModal,
    private userService: UserService,
    private permissionsService: PermissionsService
  ) { }

  refreshAttempts() {
    const contestId = this.activatedRoute.snapshot.params.contestId;
    let sieveFilters = this.problemId ? `problemId==${this.problemId}` : '';

    if (!this.canViewAnyAttemptSrc) {
      sieveFilters += `authorId==${this.currentUserId}`;
    }

    this.attemptService.apiAttemptsGet(sieveFilters, '-CreatedAt', undefined, undefined, contestId)
      .subscribe(attempts => {
        this.attempts = attempts.attempts ?? [];
      });
  }

  ngOnInit(): void {
    const contestId = this.activatedRoute.snapshot.params.contestId;
    const usersGet = this.userService.apiUsersGet()
      .pipe(tap(user => {
        this.currentUserId = user.id;
      }))
    const hasPermission = this.permissionsService.hasPermissionObservable('ManageAttempts')
      .pipe(tap(hasPermission => {
        if (!hasPermission) return;
        this.canViewAnyAttemptSrc = true;
      }))
    const canAdjustContestGrade = this.permissionsService.canAdjustContestGrade(contestId)
      .pipe(tap(canAdjust => {
        if (!canAdjust) return;
        this.canViewAnyAttemptSrc = true;
      }));
    forkJoin([usersGet, hasPermission, canAdjustContestGrade])
      .subscribe(() => this.refreshAttempts());
    this.refresh.emit(() => this.refreshAttempts());
  }

  statusToString(status: number): string {
    return Constants.attemptStatusToString(status);
  }

  private showAttemptSrc(attemptId: string) {
    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = attemptId;
    modalRef.componentInstance.contestId = this.activatedRoute.snapshot.params.contestId;
  }

  public canViewAttempt(attempt: AttemptDto): boolean {
    if (this.currentUserId === attempt.authorId) return true;

    return this.canViewAnyAttemptSrc;
  }

  public onAttemptClick(attemptId: string) {
    if (!this.canViewAttempt(this.attempts.find(a => a.id === attemptId)!)) return;

    this.showAttemptSrc(attemptId);
  }

}
