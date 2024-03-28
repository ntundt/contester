import { Component, Input, OnInit } from '@angular/core';
import {
  ContestDto,
  ContestService,
  ContestSettingsDto,
  CreateProblemCommand,
  ProblemDto,
  ProblemService,
  SchemaDescriptionDto,
  SchemaDescriptionService
} from "../../../generated/client";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {NgFor, NgIf} from "@angular/common";
import {MarkdownComponent} from "ngx-markdown";
import {faPencil, faPlus, faTrashCan, faPenToSquare} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {InputObjectNameModalComponent} from "../../shared/input-object-name-modal/input-object-name-modal.component";
import {
  DeleteConfirmationModalComponent
} from "../../shared/delete-confirmation-modal/delete-confirmation-modal.component";
import {ClaimsService} from "../../../authorization/claims.service";
import { EditTextModalComponent } from 'src/app/shared/edit-text-modal/edit-text-modal.component';

@Component({
  selector: 'app-problems',
  standalone: true,
  imports: [NgFor, RouterLink, MarkdownComponent, FaIconComponent, NgIf],
  templateUrl: './problems.component.html',
  styleUrl: './problems.component.css',
  viewProviders: [NgFor],
})
export class ProblemsComponent implements OnInit {
  public problems: Array<ProblemDto> = [];
  private schemas: Array<SchemaDescriptionDto> = [];
  
  // TODO: Make it @Input()
  public contest: ContestSettingsDto | undefined;

  public constructor(private route: ActivatedRoute, private problemService: ProblemService,
                     private modalService: NgbModal, private schemaService: SchemaDescriptionService,
                     public claimsService: ClaimsService, private contestService: ContestService) { }

  private fetchProblems() {
    this.problemService.apiProblemsGet(this.route.snapshot.params['contestId']).subscribe(problems => {
      this.problems = problems.problems || [];
    });
  }

  private fetchSchemas() {
    this.schemaService.apiSchemaDescriptionsGet().subscribe(schemas => {
      this.schemas = schemas.schemaDescriptions || [];
    });
  }

  private fetchContest() {
    const contestId = this.route.snapshot.params['contestId'];
    this.contestService.apiContestsContestIdSettingsGet(contestId).subscribe(contest => {
      this.contest = contest;
    });
  }

  public ngOnInit(): void {
    this.fetchProblems();
    this.fetchSchemas();
    this.fetchContest();
  }

  public deleteProblem(problemId: string) {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: boolean) => {
      if (!result) return;
      this.problemService.apiProblemsProblemIdDelete(problemId).subscribe(() => {
        this.fetchProblems();
      });
    });
  }

  public addProblem() {
    const modalRef = this.modalService.open(InputObjectNameModalComponent);
    modalRef.componentInstance.title = 'Add problem';
    modalRef.componentInstance.placeholder = 'Problem name';
    modalRef.result.then((result: string) => {
      if (!result) return;
      const command: CreateProblemCommand = {
        name: result,
        statement: '',
        orderMatters: false,
        floatMaxDelta: 0,
        caseSensitive: false,
        // @ts-ignore
        timeLimit: '00:00:00',
        contestId: this.route.snapshot.params['contestId'],
        schemaDescriptionId: this.schemas[0].id,
        solution: '',
        solutionDbms: 'SqlServer',
      };
      if (result) {
        this.problemService.apiProblemsPost(command).subscribe(() => {
          this.fetchProblems();
        });
      }
    });
  }

  public editDescription() {
    const modalRef = this.modalService.open(EditTextModalComponent, { size: 'lg' });
    modalRef.componentInstance.text = this.contest?.description || '';
    modalRef.result.then((result: string) => {
      if (result !== undefined) {
        this.contestService.apiContestsContestIdPut(this.route.snapshot.params['contestId'], {
          ...this.contest!,
          description: result,
          commissionMembers: this.contest?.commissionMembers?.map(x => x.id!) || [],
        }).subscribe(() => {
          this.fetchContest();
        });
      }
    });
  }

  protected readonly faPlus = faPlus;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faPencil = faPencil;
  protected readonly faPenToSquare = faPenToSquare;
}
