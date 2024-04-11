import { Component, Input, OnInit } from '@angular/core';
import {
  AttemptService,
  ContestParticipationDto,
  ContestService,
  ProblemDto,
  ProblemService,
  UserService,
} from "../../../generated/client";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {faArrowDownShortWide, faA, faPlusMinus, faTrashCan, faPencil} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgFor, NgIf} from "@angular/common";
import {MarkdownComponent} from "ngx-markdown";
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {FormsModule} from "@angular/forms";
import {PermissionsService} from "../../../authorization/permissions.service";
import {ToastsService} from "../../toasts/toasts.service";
import { Constants } from 'src/constants';
import { ProblemAttemptsComponent } from './problem-attempts/problem-attempts.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ActionConfirmationModalComponent } from 'src/app/shared/action-confirmation-modal/action-confirmation-modal.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';

@Component({
  selector: 'app-problem',
  standalone: true,
  imports: [
    FaIconComponent,
    NgIf,
    NgFor,
    MarkdownComponent,
    MonacoEditorModule,
    FormsModule,
    RouterLink,
    ProblemAttemptsComponent,
  ],
  templateUrl: './problem.component.html',
  styleUrl: './problem.component.css'
})
export class ProblemComponent implements OnInit {
  public editorOptions = {
    ...Constants.monacoDefaultOptions,
    language: 'sql',
  };

  public problem: ProblemDto = {
    name: '',
    statement: '',
    caseSensitive: false,
    orderMatters: false,
    floatMaxDelta: 0,
    availableDbms: ['SqlServer'],
  };

  public awaitingCheckResult: boolean = false;

  public selectedContestantSolutionDialect: string = this.problem.availableDbms?.[0] ?? '';
  public contestantSolution: string = '';

  public selectedSchemaDescription: string | undefined;

  contest: ContestParticipationDto | undefined;
  userHasManageContestsClaim: boolean = false;
  userIsContestant: boolean = false;

  contestId: string | undefined;
  problemId: string | undefined;

  currentUserId: string | undefined;

  refreshProblemAttempts: () => void = () => {};
  handleRefreshCallback(event: () => void) {
    this.refreshProblemAttempts = event;
  }

  public constructor(
    private route: ActivatedRoute,
    private problemService: ProblemService,
    private attemptService: AttemptService,
    public permissionsService: PermissionsService,
    private toastsService: ToastsService,
    private contestService: ContestService,
    private modalService: NgbModal,
    private userService: UserService,
  ) { }

  submitSolution() {
    this.awaitingCheckResult = true;
    this.attemptService.apiAttemptsPost({
      problemId: this.problem.id!,
      solution: this.contestantSolution,
      dbms: this.selectedContestantSolutionDialect,
    }).subscribe(res => {
      this.awaitingCheckResult = false;
      this.refreshProblemAttempts();
      this.toastsService.show({
        header: 'Attempt submitted',
        body: `Your attempt was submitted. Status is ${Constants.attemptStatusToString(res.status!)}.`,
      })
    });
  }

  public onContestantSubmit(): void {
    const contestId = this.route.snapshot.params['contestId'];
    const sieveFilters = `ProblemId==${this.problemId}, AuthorId==${this.currentUserId}, Status==Accepted`;
    this.attemptService.apiAttemptsGet(sieveFilters, undefined, undefined, undefined, contestId).subscribe(res => {
      if (res.attempts?.length === 0) {
        this.submitSolution();
      } else {
        const modalRef = this.modalService.open(ActionConfirmationModalComponent, { centered: true });
        modalRef.componentInstance.title = 'Confirm submission';
        modalRef.componentInstance.message = 'You have already solved this problem. Only the latest submission will be considered. Are you sure you want to submit a new solution?';
        modalRef.result.then((result) => {
          if (!result) return;
          this.submitSolution();
        });
      }
    });
  }

  public ngOnInit(): void {
    this.contestId = this.route.snapshot.params['contestId'];
    this.problemId = this.route.snapshot.params['problemId'];

    this.problemService.apiProblemsGet(this.contestId).subscribe(res => {
      this.problem = res.problems?.find(p => p.id === this.problemId) ?? this.problem;
      this.selectedContestantSolutionDialect = this.problem.availableDbms?.[0] ?? '';
    });

    this.contestService.apiContestsGet(undefined, `id==${this.contestId}`).subscribe(res => {
      this.contest = res.contests?.[0];
      this.userIsContestant = this.contest?.userParticipates ?? false;
    });

    this.permissionsService.hasPermissionObservable('ManageContests').subscribe(res => {
      this.userHasManageContestsClaim = res;
    });

    this.userService.apiUsersGet().subscribe(res => {
      this.currentUserId = res.id;
    });
  }

  canSubmitSolution(): boolean {
    return this.contest?.isPublic || this.userHasManageContestsClaim || this.userIsContestant;
  }

  submitSolutionDisabled(): boolean {
    return !this.canSubmitSolution() || this.contestantSolution === '' || this.selectedContestantSolutionDialect === '';
  }

  protected readonly faArrowDownShortWide = faArrowDownShortWide;
  protected readonly faA = faA;
  protected readonly faPlusMinus = faPlusMinus;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faPencil = faPencil;
}
