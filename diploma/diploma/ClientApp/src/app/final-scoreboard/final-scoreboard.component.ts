import { Component, OnInit } from '@angular/core';
import { ScoreboardComponent } from '../main-area/scoreboard/scoreboard.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { ClaimsService } from '../../authorization/claims.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { ContestDto, ContestService, GetScoreboardApprovalStatusQueryResult, ScoreboardService, UserService } from 'src/generated/client';
import { faFileArrowDown } from '@fortawesome/free-solid-svg-icons';
import { NgbModal, NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { ActionConfirmationModalComponent } from '../shared/action-confirmation-modal/action-confirmation-modal.component';


@Component({
  selector: 'app-final-scoreboard',
  standalone: true,
  imports: [ScoreboardComponent, RouterLink, NgIf, FaIconComponent, NgbPopover],
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
    public claimsService: ClaimsService,
    public contestService: ContestService,
    public scoreboardService: ScoreboardService,
    public userService: UserService,
    public modalService: NgbModal,
  ) { }

  private getApprovalStatus() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.scoreboardService.apiScoreboardApprovalStatusGet(contestId).subscribe(res => {
      this.approvalStatus = res;
    });
  }

  public ngOnInit() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.claimsService.hasClaimObservable('ManageContests').subscribe(res => {
      this.hasManageContestsClaim = res;
    });
    this.claimsService.canAdjustContestGrade(contestId).subscribe(res => {
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

  downloadReport() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.contestService.apiContestsContestIdReportGet(contestId).subscribe(res => {
      const blob = new Blob([JSON.stringify(res, null, 2)], { type: 'application/octet-stream' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `report-${contestId}.json`;
      a.click();
      window.URL.revokeObjectURL(url);
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
    return new Date(this.contest?.finishDate ?? '').getTime() < Date.now();
  }

  protected readonly faFileArrowDown = faFileArrowDown;
}
