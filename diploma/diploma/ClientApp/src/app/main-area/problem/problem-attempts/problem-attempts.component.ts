import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgbModal, NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { AttemptDto, AttemptService, UserDto, UserService } from 'src/generated/client';
import { Constants } from 'src/constants';
import { AttemptSrcViewModalComponent } from 'src/app/shared/attempt-src-view-modal/attempt-src-view-modal.component';
import { PermissionsService } from 'src/authorization/permissions.service';
import { Observable, forkJoin, tap } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { faFilter, faTimes } from '@fortawesome/free-solid-svg-icons';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { UserSelectionModalComponent } from '../../settings/user-selection-modal/user-selection-modal.component';

@Component({
  selector: 'app-problem-attempts',
  standalone: true,
  imports: [
    NgIf,
    NgbPopover,
    DatePipe,
    NgFor,
    TranslateModule,
    FaIconComponent,
  ],
  templateUrl: './problem-attempts.component.html',
  styleUrl: './problem-attempts.component.css'
})
export class ProblemAttemptsComponent implements OnInit {

  attempts: AttemptDto[] = [];

  private currentUserId: string | undefined;
  protected canViewAnyAttemptSrc: boolean = false;
  @Input() problemId: string | undefined;
  @Output() refresh: EventEmitter<() => void> = new EventEmitter();

  protected filterByAuthor: UserDto[] = [];
  protected filterByStatus: number[] = [];

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
      sieveFilters += (sieveFilters ? ',' : '') + `authorId==${this.currentUserId}`;
    }

    if (this.filterByAuthor.length > 0) {
      sieveFilters += (sieveFilters ? ',' : '') + `authorId==${this.filterByAuthor.map(a => a.id!).join('|')}`;
    }

    if (this.filterByStatus.length > 0) {
      sieveFilters += (sieveFilters ? ',' : '') + `status==${this.filterByStatus.join('|')}`;
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

  private filterByAuthorAdd(author: UserDto) {
    if (this.filterByAuthor.some(a => a.id === author.id)) return;
    this.filterByAuthor.push(author);
    this.refreshAttempts();
  }

  filterByAuthorAddById(authorId: string) {
    this.userService.apiUsersUserIdGet(authorId)
      .subscribe(author => this.filterByAuthorAdd(author));
  }

  filterByAuthorRemove(id: string) {
    this.filterByAuthor = this.filterByAuthor.filter(a => a.id !== id);
    this.refreshAttempts();
  }

  openFilterByAuthorAddUserModal() {
    const modal = this.modalService.open(UserSelectionModalComponent);
    modal.componentInstance.excludedUserIds = this.filterByAuthor.map(a => a.id!);
    modal.result.then((result: UserDto) => {
      if (!result) return;
      this.filterByAuthorAdd(result);
    });
  }

  displayFilterRow() {
    return this.filterByAuthor.length > 0 || this.filterByStatus.length > 0;
  }

  filterByStatusAdd(status: number) {
    if (this.filterByStatus.includes(status)) return;
    this.filterByStatus.push(status);
    this.refreshAttempts();
  }

  filterByStatusRemove(status: number) {
    this.filterByStatus = this.filterByStatus.filter(s => s !== status);
    this.refreshAttempts();
  }

  protected readonly faFilter = faFilter;
  protected readonly faTimes = faTimes;
}
