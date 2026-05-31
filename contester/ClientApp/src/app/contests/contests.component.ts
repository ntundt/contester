import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ContestParticipationDto, ContestService } from '../../generated/client';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faChartSimple, faClock, faPlus } from '@fortawesome/free-solid-svg-icons';
import { RouterLink } from '@angular/router';
import { AuthenticationHelperService } from '../../authorization/authentication-helper.service';
import { DatePipe } from '@angular/common';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { InputObjectNameModalComponent } from '../shared/input-object-name-modal/input-object-name-modal.component';
import { PermissionsService } from '../../authorization/permissions.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-contests',
  standalone: true,
  imports: [
    FaIconComponent,
    RouterLink,
    DatePipe,
    TranslateModule,
  ],
  templateUrl: './contests.component.html',
  styleUrl: './contests.component.css',
})
export class ContestsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly contests = signal<ContestParticipationDto[]>([]);

  readonly currentContests = computed(() =>
    this.contests().filter(
      (contest) => new Date(contest.finishDate!).getTime()! > Date.now(),
    ),
  );

  readonly finishedContests = computed(() =>
    this.contests().filter(
      (contest) => new Date(contest.finishDate!).getTime()! < Date.now(),
    ),
  );

  public constructor(
    private contestService: ContestService,
    public authenticationHelperService: AuthenticationHelperService,
    private modalService: NgbModal,
    public permissionsService: PermissionsService,
  ) { }

  public ngOnInit() {
    this.fetchContests();

    this.authenticationHelperService
      .getCredentials()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cdr.markForCheck());
  }

  private fetchContests() {
    this.contestService
      .apiContestsGet(undefined, undefined, '-StartDate')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((res) => {
        this.contests.set(res.contests ?? []);
        this.cdr.markForCheck();
      });
  }

  public createContest() {
    const modalRef = this.modalService.open(InputObjectNameModalComponent);
    modalRef.componentInstance.title = 'Create contest';
    modalRef.componentInstance.placeholder = 'Contest name';
    modalRef.componentInstance.confirmButtonText = 'Create';

    modalRef.result.then((result) => {
      this.contestService
        .apiContestsPost({
          name: result,
          description: '',
          participants: [],
          startDate: new Date(Date.now() + 24 * 60 * 60 * 1000),
          endDate: new Date(Date.now() + 24 * 60 * 60 * 1000 + 60 * 60 * 1000),
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.fetchContests());
    });
  }

  shouldOpenContestOnceItStarts(contest: ContestParticipationDto) {
    return (
      !this.permissionsService.hasPermission('ManageContests') &&
      contest.isPublic! &&
      new Date(contest.startDate!).getTime()! > Date.now()
    );
  }

  public shouldOpenContestApplication(contest: ContestParticipationDto) {
    return (
      !this.permissionsService.hasPermission('ManageContests') &&
      !contest.isPublic! &&
      new Date(contest.startDate!).getTime()! > Date.now()
    );
  }

  public tooLateToApply(contest: ContestParticipationDto) {
    return (
      new Date(contest.startDate!).getTime()! < Date.now() &&
      !contest.isPublic! &&
      !contest.userParticipates! &&
      !this.permissionsService.hasPermission('ManageContests')
    );
  }

  public shouldOpenContest(contest: ContestParticipationDto) {
    return (
      this.permissionsService.hasPermission('ManageContests') ||
      ((contest.isPublic || contest.userParticipates) &&
        new Date(contest.startDate!).getTime()! < Date.now())
    );
  }

  protected readonly faClock = faClock;
  protected readonly faChartSimple = faChartSimple;
  protected readonly faPlus = faPlus;
}
