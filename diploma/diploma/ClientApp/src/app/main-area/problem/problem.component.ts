import { Component, Input, OnInit } from '@angular/core';
import {
  AttemptService,
  ContestParticipationDto,
  ContestService,
  ProblemDto,
  ProblemService,
} from "../../../generated/client";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {faArrowDownShortWide, faA, faPlusMinus, faTrashCan, faPencil} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgFor, NgIf} from "@angular/common";
import {MarkdownComponent} from "ngx-markdown";
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {FormsModule} from "@angular/forms";
import {ClaimsService} from "../../../authorization/claims.service";
import {ToastsService} from "../../toasts/toasts.service";
import { Constants } from 'src/constants';
import { ProblemAttemptsComponent } from './problem-attempts/problem-attempts.component';

@Component({
  selector: 'app-problem',
  standalone: true,
  imports: [
    FaIconComponent,
    NgIf,
    NgFor,
    MarkdownComponent,
    CodeEditorModule,
    FormsModule,
    RouterLink,
    ProblemAttemptsComponent,
  ],
  templateUrl: './problem.component.html',
  styleUrl: './problem.component.css'
})
export class ProblemComponent implements OnInit {
  public contestantSolutionSqlCodeModel: CodeModel = { language: 'sql', value: 'SELECT * FROM [Employees];', uri: '1' };

  public problem: ProblemDto = {
    name: '',
    statement: '',
    caseSensitive: false,
    orderMatters: false,
    floatMaxDelta: 0,
    availableDbms: ['SqlServer'],
  };

  public selectedContestantSolutionDialect: string = this.problem.availableDbms?.[0] ?? '';
  public contestantSolution: string = '';

  public selectedSchemaDescription: string | undefined;

  contest: ContestParticipationDto | undefined;
  userHasManageContestsClaim: boolean = false;
  userIsContestant: boolean = false;

  contestId: string | undefined;
  problemId: string | undefined;

  public constructor(
    private route: ActivatedRoute,
    private problemService: ProblemService,
    private attemptService: AttemptService,
    public claimsService: ClaimsService,
    private toastsService: ToastsService,
    private contestService: ContestService) { }

  public onContestantSubmit(): void {
    this.attemptService.apiAttemptsPost({
      problemId: this.problem!.id!,
      solution: this.contestantSolution,
      dbms: this.selectedContestantSolutionDialect,
    }).subscribe(res => {
      this.toastsService.show({
        header: 'Attempt submitted',
        body: `Your attempt was submitted. Status is ${Constants.attemptStatusToString(res.status!)}.`,
      })
    });
  }

  public onContestantSolutionChanged(code: string): void {
    this.contestantSolution = code;
  }

  public ngOnInit(): void {
    this.contestId = this.route.snapshot.params['contestId'];
    this.problemId = this.route.snapshot.params['problemId'];

    this.problemService.apiProblemsGet(this.contestId).subscribe(res => {
      this.problem = res.problems?.[0] ?? this.problem;
    });

    this.contestService.apiContestsGet(undefined, `id==${this.contestId}`).subscribe(res => {
      this.contest = res.contests?.[0];
      this.userIsContestant = this.contest?.userParticipates ?? false;
    });

    this.claimsService.hasClaimObservable('ManageContests').subscribe(res => {
      this.userHasManageContestsClaim = res;
    });
  }

  canSubmitSolution(): boolean {
    return this.contest?.isPublic || this.userHasManageContestsClaim || this.userIsContestant;
  }

  protected readonly faArrowDownShortWide = faArrowDownShortWide;
  protected readonly faA = faA;
  protected readonly faPlusMinus = faPlusMinus;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faPencil = faPencil;
}
