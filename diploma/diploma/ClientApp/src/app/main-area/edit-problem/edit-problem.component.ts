import { Component, OnInit } from '@angular/core';
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {FormsModule} from "@angular/forms";
import {NgForOf} from "@angular/common";
import {ProblemDto, ProblemService, SchemaDescriptionDto, SchemaDescriptionService} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {ToastsService} from "../../toasts/toasts.service";

@Component({
  selector: 'app-edit-problem',
  standalone: true,
    imports: [
        CodeEditorModule,
        FormsModule,
        NgForOf
    ],
  templateUrl: './edit-problem.component.html',
  styleUrl: './edit-problem.component.css'
})
export class EditProblemComponent implements OnInit {
  public expectedSolutionSqlCodeModel: CodeModel = { language: 'sql', value: 'SELECT * FROM [Employees];', uri: '2' };
  public markdownCodeModel: CodeModel = { language: 'markdown', value: '## Write problem statement here', uri: '3' };

  public problem: ProblemDto = {
    name: '',
    statement: '',
    caseSensitive: false,
    orderMatters: false,
    floatMaxDelta: 0,
    maxGrade: 0,
    ordinal: 0,
    availableDbms: ['SqlServer'],
  };

  public selectedExpectedSolutionDialect: string = this.problem.availableDbms?.[0] ?? '';
  public expectedSolution: string = '';

  public schemaDescriptions: Array<SchemaDescriptionDto> | undefined;
  public selectedSchemaDescription: string | undefined;

  public latestError: string = '';

  public constructor(
    private problemService: ProblemService, private schemaDescriptionService: SchemaDescriptionService,
    private route: ActivatedRoute, private toastsService: ToastsService,
  ) { }

  public ngOnInit(): void {
    const contestId = this.route.snapshot.params['contestId'];
    const problemId = this.route.snapshot.params['problemId'];

    this.problemService.apiProblemsGet(contestId).subscribe(res => {
      if (res.problems == null) return;
      this.problem = res.problems.find(problem => problem.id == problemId) ?? this.problem;
      this.markdownCodeModel = {
        ...this.markdownCodeModel,
        value: this.problem?.statement ?? '',
      };
    });

    this.problemService.apiProblemsProblemIdExpectedSolutionGet(problemId).subscribe(res => {
      this.expectedSolution = res.solution ?? '';
      this.selectedExpectedSolutionDialect = res.dbms ?? '';
      this.expectedSolutionSqlCodeModel = {
        ...this.expectedSolutionSqlCodeModel,
        value: this.expectedSolution,
      };
    });

    this.schemaDescriptionService.apiSchemaDescriptionsGet(`contestId==${contestId}`).subscribe(res => {
      this.schemaDescriptions = res.schemaDescriptions;
      this.selectedSchemaDescription = this.schemaDescriptions?.[0].id;
    });
  }

  public onAdminSubmit(): void {
    this.problemService.apiProblemsProblemIdPut(this.problem!.id!, {
      ...this.problem,
      statement: this.markdownCodeModel.value,
      solution: this.expectedSolution,
      solutionDbms: this.selectedExpectedSolutionDialect,
    })
    .subscribe({
      next: () => {
        this.toastsService.show({
          header: 'Problem updated',
          body: `Problem ${this.problem.name} has been updated`,
          delay: 3000,
        });
        this.latestError = '';
      },
      error: (err) => {
        this.latestError = err.error.message;
      },
    });
  }

  public onExpectedSolutionChanged(code: string): void {
    this.expectedSolution = code;
  }

  public problemStatementChanged(code: any): void {
    this.problem!.statement = code;
  }
}
