import { Component, Input, OnInit } from '@angular/core';
import {
  ContestService,
  ContestSettingsDto,
  CreateProblemCommand,
  ProblemDto,
  ProblemService,
  SchemaDescriptionService
} from "../../../generated/client";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {MarkdownComponent} from "ngx-markdown";
import {
  faPencil,
  faPlus,
  faTrashCan,
  faPenToSquare,
  faStar,
  faInbox,
  faUserCheck, faArrowRight
} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {InputObjectNameModalComponent} from "../../shared/input-object-name-modal/input-object-name-modal.component";
import {
  DeleteConfirmationModalComponent
} from "../../shared/delete-confirmation-modal/delete-confirmation-modal.component";
import {PermissionsService} from "../../../authorization/permissions.service";
import { EditTextModalComponent } from 'src/app/shared/edit-text-modal/edit-text-modal.component';
import { TranslateModule } from '@ngx-translate/core';
import {DeclensionPipe} from "../../pipes/declension-pipe";
import {ContestEventsService} from "../../services/contest-events-service";

@Component({
  selector: 'app-problems',
  standalone: true,
  imports: [
    RouterLink,
    MarkdownComponent,
    FaIconComponent,
    TranslateModule,
    DeclensionPipe,
  ],
  templateUrl: './problems.component.html',
  styleUrl: './problems.component.css',
})
export class ProblemsComponent implements OnInit {
  public problems: Array<ProblemDto> = [];

  // TODO: Make it @Input()
  public contest: ContestSettingsDto | undefined;

  protected loadingProblems = true;

  public constructor(
    private route: ActivatedRoute,
    private problemService: ProblemService,
    private modalService: NgbModal,
    private schemaService: SchemaDescriptionService,
    private contestService: ContestService,
    private events: ContestEventsService,
    public permissionsService: PermissionsService,
  ) { }

  private fetchProblems() {
    this.loadingProblems = true;
    this.problemService.apiProblemsGet(this.route.snapshot.params['contestId']).subscribe(problems => {
      this.problems = problems.problems || [];
      this.loadingProblems = false;
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
    this.fetchContest();
  }

  public deleteProblem(problemId: string) {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: boolean) => {
      if (!result) return;
      this.problemService.apiProblemsProblemIdDelete(problemId).subscribe(() => {
        this.events.problemsChanged();
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
        solution: '',
        solutionDbms: 'SqlServer',
      };
      if (result) {
        this.problemService.apiProblemsPost(command).subscribe(() => {
          this.events.problemsChanged();
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
  protected readonly faStar = faStar;
  protected readonly faInbox = faInbox;
  protected readonly faUserCheck = faUserCheck;
  protected readonly faArrowRight = faArrowRight;
}
