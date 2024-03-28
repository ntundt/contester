import { Component, OnInit } from '@angular/core';
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {FormsModule} from "@angular/forms";
import {NgForOf} from "@angular/common";
import {AttachedFileService, ProblemDto, ProblemService, SchemaDescriptionDto, SchemaDescriptionService} from "../../../generated/client";
import {ActivatedRoute, Router} from "@angular/router";
import {ToastsService} from "../../toasts/toasts.service";
import {EditorComponent, MonacoEditorModule} from "ngx-monaco-editor-v2";
import { faChevronLeft, faPaperclip } from '@fortawesome/free-solid-svg-icons';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Constants } from 'src/constants';
import { EditorWithAttachmentsComponent } from 'src/app/shared/editor-with-attachments/editor-with-attachments.component';
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { faQuestionCircle } from '@fortawesome/free-regular-svg-icons';

@Component({
  selector: 'app-edit-problem',
  standalone: true,
  imports: [
      CodeEditorModule,
      FormsModule,
      NgForOf,
      MonacoEditorModule,
      FaIconComponent,
      EditorWithAttachmentsComponent,
      NgbPopover,
  ],
  templateUrl: './edit-problem.component.html',
  styleUrl: './edit-problem.component.css'
})
export class EditProblemComponent implements OnInit {
  public expectedSolutionSqlCodeModel: CodeModel = { language: 'sql', value: 'SELECT * FROM [Employees];', uri: '2' };
  public editorOptions = Constants.monacoDefaultOptions;

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

  private editorInstance: any;

  public constructor(
    private problemService: ProblemService, 
    private schemaDescriptionService: SchemaDescriptionService,
    private route: ActivatedRoute,
    private router: Router,
    private toastsService: ToastsService, 
    private attachedFiles: AttachedFileService,
  ) { }

  public ngOnInit(): void {
    const contestId = this.route.snapshot.params['contestId'];
    const problemId = this.route.snapshot.params['problemId'];

    this.problemService.apiProblemsGet(contestId).subscribe(res => {
      if (res.problems == null) return;
      this.problem = res.problems.find(problem => problem.id == problemId) ?? this.problem;
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

  public onProblemStatementAttachFile(): void {
    const reader = new FileReader();
    const fileInput = document.createElement('input');
    reader.onload = () => {
      if (!reader.result) return;
      const file = new Blob([reader.result], { type: 'text/plain',  });
      const originalFileName = fileInput.files?.item(0)?.name;
      this.attachedFiles.apiFilePostForm(file, originalFileName).subscribe(res => {
        const url = res.fileUrl;
        this.editorInstance.trigger('keyboard', 'type', { text: `[${originalFileName}](${url})` });
      });
    }
    fileInput.type = 'file';
    fileInput.onchange = () => {
      const file = fileInput.files?.item(0);
      if (file) {
        reader.readAsText(file);
      }
    }

    fileInput.click();
  }

  public onProblemStatementEditorInit(editor: any): void {
    this.editorInstance = editor;
  }

  public backToProblem() {
    this.router.navigate(['../'], { relativeTo: this.route });
  }

  protected readonly faPaperclip = faPaperclip;
  protected readonly faChevronLeft = faChevronLeft;
  protected readonly faQuestionCircle = faQuestionCircle;
}
