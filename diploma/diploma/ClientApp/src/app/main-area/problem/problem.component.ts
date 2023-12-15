import { Component, OnInit } from '@angular/core';
import {
  AttemptService,
  ProblemDto,
  ProblemService,
  SchemaDescriptionDto,
  SchemaDescriptionService
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

  public constructor(
    private route: ActivatedRoute,
    private problemService: ProblemService,
    private attemptService: AttemptService,
    public claimsService: ClaimsService,
    private toastsService: ToastsService) { }

  private statusToString(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Syntax error';
      case 2: return 'Wrong answer';
      case 3: return 'Wrong output format';
      case 4: return 'Time limit exceeded';
      case 5: return 'Accepted';
      default: return 'Unknown';
    }
  }

  public onContestantSubmit(): void {
    this.attemptService.apiAttemptsPost({
      problemId: this.problem!.id!,
      solution: this.contestantSolution,
      dbms: this.selectedContestantSolutionDialect,
    }).subscribe(res => {
      this.toastsService.show({
        header: 'Attempt submitted',
        body: `Your attempt was submitted. Status is ${this.statusToString(res.status!)}.`,
      })
    });
  }

  public onContestantSolutionChanged(code: string): void {
    this.contestantSolution = code;
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      const contestId = params['contestId'];
      const problemId = params['problemId'];

      if (contestId == null || problemId == null) return;

      this.problemService.apiProblemsGet(contestId).subscribe(res => {
        if (res.problems == null) return;
        this.problem = res.problems.find(problem => problem.id == problemId) ?? this.problem;
      });
    });
  }

  protected readonly faArrowDownShortWide = faArrowDownShortWide;
  protected readonly faA = faA;
  protected readonly faPlusMinus = faPlusMinus;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faPencil = faPencil;
}
