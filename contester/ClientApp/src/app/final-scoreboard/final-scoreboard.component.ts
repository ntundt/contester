import { Component, OnInit } from '@angular/core';
import { ScoreboardComponent } from '../main-area/scoreboard/scoreboard.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { PermissionsService } from '../../authorization/permissions.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { ContestDto, ContestReportDto, ContestService, GetScoreboardApprovalStatusQueryResult, ScoreboardService, UserService } from 'src/generated/client';
import { faFileCsv, faFileLines } from '@fortawesome/free-solid-svg-icons';
import { NgbModal, NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { ActionConfirmationModalComponent } from '../shared/action-confirmation-modal/action-confirmation-modal.component';
import { AuthorizationService } from 'src/authorization/authorization.service';
import { TranslateModule } from '@ngx-translate/core';


@Component({
  selector: 'app-final-scoreboard',
  standalone: true,
  imports: [
    ScoreboardComponent,
    RouterLink,
    NgIf,
    FaIconComponent,
    NgbPopover,
    TranslateModule,
  ],
  templateUrl: './final-scoreboard.component.html',
  styleUrl: './final-scoreboard.component.css'
})
export class FinalScoreboardComponent implements OnInit {
  public hasManageContestsClaim: boolean = false;
  public canAdjustContestGrades: boolean = false;

  public userId: string | undefined;

  public approvalStatus: GetScoreboardApprovalStatusQueryResult = { };

  public contest: ContestDto | undefined;

  constructor(
    public activatedRoute: ActivatedRoute,
    public permissionsService: PermissionsService,
    public contestService: ContestService,
    public scoreboardService: ScoreboardService,
    public userService: UserService,
    public modalService: NgbModal,
    private authService: AuthorizationService,
  ) { }

  private getApprovalStatus() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.scoreboardService.apiScoreboardApprovalStatusGet(contestId).subscribe(res => {
      this.approvalStatus = res;
    });
  }

  public ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      return;
    }

    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.permissionsService.hasPermissionObservable('ManageContests').subscribe(res => {
      this.hasManageContestsClaim = res;
    });
    this.permissionsService.canAdjustContestGrade(contestId).subscribe(res => {
      this.canAdjustContestGrades = res;
    });
    this.userService.apiUsersGet().subscribe(res => {
      this.userId = res.id;
    });
    this.getApprovalStatus();
    this.contestService.apiContestsGet(undefined, `id==${contestId}`).subscribe(res => {
      this.contest = res.contests?.[0];
    });
  }

  fetchReport() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    return this.contestService.apiContestsContestIdReportGet(contestId);
  }

  downloadStringAsFile(data: string, filename: string) {
    const bom = new Uint8Array([0xEF, 0xBB, 0xBF]);
    const blob = new Blob([bom, data], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  downloadJsonReport() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    const filename = `report-${contestId}.json`;
    this.fetchReport().subscribe(res => {
      this.downloadStringAsFile(JSON.stringify(res, null, 2), filename);
    });
  }

  private convertReportToCsv(report: ContestReportDto) {
    const csv = [];
    const header = Object.keys(report.participants?.[0] ?? {});
    csv.push(header.join(';'));
    report.participants?.forEach(p => {
      const row = header.map(h => p[h as keyof typeof p]);
      csv.push(row.join(';'));
    });
    return csv.join('\n');
  }

  downloadCsvReport() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    const filename = `report-${contestId}.csv`;
    this.fetchReport().subscribe(res => {
      const csv = this.convertReportToCsv(res);
      this.downloadStringAsFile(csv, filename);
    });
  }

  currentUserApproved() {
    return this.approvalStatus.approvedUsers?.some(u => u.id === this.userId);
  }

  getUnapprovedUsersNames() {
    return this.approvalStatus.notApprovedUsers?.map(u => `${u.firstName} ${u.lastName}`).join(', ');
  }

  approveScoreboard() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.scoreboardService.apiScoreboardApprovePost(contestId).subscribe(() => {
      this.getApprovalStatus();
    });
  }

  onApproveClick() {
    const modalRef = this.modalService.open(ActionConfirmationModalComponent, { centered: true });
    modalRef.componentInstance.title = 'Approve the results';
    modalRef.componentInstance.message = 'After you approve the results, it will be locked and no further changes from you will be possible. Are you sure you want to proceed?';
    modalRef.result.then((result) => {
      if (!result) return;
      this.approveScoreboard();
    });
  }

  isNotApprovedBySomeone() {
    return !!this.approvalStatus.notApprovedUsers && (this.approvalStatus.notApprovedUsers?.length > 0);
  }

  contestIsFinished() {
    return new Date(this.contest?.finishDate + 'Z').getTime() < Date.now();
  }

  protected readonly faFileLines = faFileLines;
  protected readonly faFileCsv = faFileCsv;
}
