import {Component, Input, OnInit} from '@angular/core';
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {NgbActiveModal, NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {AttemptService, AttemptStatus, AuthenticationService, GradeAdjustmentDto, GradeAdjustmentsService, SingleAttemptDto, UserService} from "../../../generated/client";
import {NgIf, NgFor, DatePipe} from "@angular/common";
import { PermissionsService } from 'src/authorization/permissions.service';
import { FormsModule } from '@angular/forms';
import { Constants } from 'src/constants';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheck, faTimes } from '@fortawesome/free-solid-svg-icons';
import { ToastsService } from 'src/app/toasts/toasts.service';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from "@angular/router";

@Component({
  selector: 'app-attempt-src-view-modal',
  standalone: true,
  imports: [
    CodeEditorModule,
    FormsModule,
    NgIf,
    NgFor,
    FaIconComponent,
    DatePipe,
    TranslateModule,
  ],
  templateUrl: './attempt-src-view-modal.component.html',
  styleUrl: './attempt-src-view-modal.component.css'
})
export class AttemptSrcViewModalComponent implements OnInit {
  private static codeModelUriCounter = 0;
  public srcCodeModel: CodeModel = {
    language: 'sql',
    value: '',
    uri: `code-editor-${AttemptSrcViewModalComponent.codeModelUriCounter++}`,
  };
  public attempt: SingleAttemptDto | undefined;
  public canAdjustGrade: boolean = false;
  public gradeAdjustments: Array<GradeAdjustmentDto> = [];

  public yourGrade: number | undefined;
  public yourComment: string | undefined;

  @Input() public attemptId: string | undefined;
  @Input() public contestId: string | undefined;
  public readonly AcceptedAttemptStatus = AttemptStatus.NUMBER_5;
  public readonly Constants_EmptyGuid = Constants.EmptyGuid;
  public readonly Constants_OriginalityCheckThreshold = Constants.OriginalityCheckThreshold;
  public readonly Constants_MaxInt = Constants.MaxInt32;
  public readonly faCheck = faCheck;
  public readonly faTimes = faTimes;

  public constructor(
    public activeModal: NgbActiveModal,
    private modalService: NgbModal,
    public attemptService: AttemptService,
    public gradeService: GradeAdjustmentsService,
    public permissionsService: PermissionsService,
    public userService: UserService,
    private toastsService: ToastsService,
    private router: Router,
  ) { }

  public confirm() {
    this.activeModal.close();
  }

  public adjustGrade() {
    this.gradeService.apiGradeAdjustmentsPost({
      attemptId: this.attemptId ?? '',
      comment: this.yourComment,
      grade: this.yourGrade ?? 0,
    }).subscribe(() => {
      this.ngOnInit();
    });
  }

  public ngOnInit() {
    this.attemptService.apiAttemptsAttemptIdGet(this.attemptId ?? '').subscribe(res => {
      this.srcCodeModel = {
        ...this.srcCodeModel,
        value: res.solution ?? '',
        uri: res.id ?? '',

      };
      this.attempt = res;
    });

    this.permissionsService.canAdjustContestGrade(this.contestId ?? '').subscribe(res => {
      this.canAdjustGrade = res;
    });

    this.gradeService.apiGradeAdjustmentsGet(this.attemptId ?? '').subscribe(res => {
      this.gradeAdjustments = res;
      this.userService.apiUsersGet().subscribe(user => {
        const userId = user.id;
        const grade = res.find(x => x.userId === userId);
        this.yourGrade = grade?.grade;
        this.yourComment = grade?.comment;
      });
    });
  }

  public showMostSimilar() {
    this.activeModal.close();
    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = this.attempt?.originalAttemptId;
    modalRef.componentInstance.contestId = this.contestId;
  }

  public waitForReevaluation: boolean = false;
  reEvaluate() {
    this.waitForReevaluation = true;
    this.attemptService.apiAttemptsAttemptIdPut(this.attemptId ?? '').subscribe({
      next: (res) => {
        this.waitForReevaluation = false;
        this.toastsService.show({
          header: 'Re-evaluation complete',
          body: `The attempt has been re-evaluated. New status is ${Constants.attemptStatusToString(res.status!)}.`,
        });
      },
      error: () => {
        this.waitForReevaluation = false;
      }
    });
  }

  showResultSets() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['/attempts', this.attemptId ?? '', 'result-set']));
    window.open(url, '_blank');
  }

  getOriginalityPercentage() {
    const preparedSolution = this.attempt?.solution?.replace(/\s+/g, ' ').toLowerCase();
    return ((this.attempt?.originality ?? 1) / (preparedSolution?.length ?? 1) * 100).toFixed(2);
  }
}
