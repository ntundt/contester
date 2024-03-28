import {Component, OnInit} from '@angular/core';
import {AttemptStatus, ScoreboardEntryDto, ScoreboardProblemEntryDto, ScoreboardService, UserService} from "../../../generated/client";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {NgForOf, NgIf} from "@angular/common";
import {faCheck, faMinus, faTimes} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import { ClaimsService } from 'src/authorization/claims.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AttemptSrcViewModalComponent } from 'src/app/shared/attempt-src-view-modal/attempt-src-view-modal.component';
import { Constants } from 'src/constants';
import { AuthorizationService } from 'src/authorization/authorization.service';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [NgForOf, FaIconComponent, NgIf, RouterLink],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.css'
})
export class ScoreboardComponent implements OnInit {
  public scoreboard: Array<ScoreboardEntryDto> = [];

  public userId: string | undefined;

  private canViewAnyAttemptSrc: boolean = false;

  public faCheck = faCheck;
  public faTimes = faTimes;
  public faMinus = faMinus;

  public constructor(
    private scoreboardService: ScoreboardService,
    private activatedRoute: ActivatedRoute,
    private claimsService: ClaimsService,
    private modalService: NgbModal,
    private userService: UserService,
    private authorizationService: AuthorizationService,
  ) { }

  public ngOnInit() {
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.scoreboardService.apiScoreboardGet(contestId).subscribe(scoreboard => {
      this.scoreboard = scoreboard.rows || [];
    });

    if (!this.authorizationService.isAuthenticated()) return;

    this.userService.apiUsersGet().subscribe(user => {
      this.userId = user.id;
    });

    this.claimsService.canAdjustContestGrade(contestId).subscribe(canAdjust => {
      if (!canAdjust) return;
      this.canViewAnyAttemptSrc = true;
    });

    this.claimsService.hasClaimObservable('ManageAttempts').subscribe(hasClaim => {
      if (!hasClaim) return;
      this.canViewAnyAttemptSrc = true;
    });
  }

  private showSolvingAttemptSrc(entry: ScoreboardProblemEntryDto) {
    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = entry.solvingAttemptId;
    modalRef.componentInstance.contestId = this.activatedRoute.snapshot.params['contestId'];
  }

  public canViewSrc(entry: ScoreboardProblemEntryDto, row: ScoreboardEntryDto) {
    if (entry.solvingAttemptId === Constants.EmptyGuid) return false;

    if (row.userId === this.userId) return true;

    if (this.canViewAnyAttemptSrc) return true;

    return false;
  }

  public onCellClick(entry: ScoreboardProblemEntryDto, row: ScoreboardEntryDto) {
    if (!this.canViewSrc(entry, row)) return;

    this.showSolvingAttemptSrc(entry);
  }

  protected readonly AttemptStatus = AttemptStatus;
}
