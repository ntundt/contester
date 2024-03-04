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

  public faCheck = faCheck;
  public faTimes = faTimes;
  public faMinus = faMinus;

  public constructor(
    private scoreboardService: ScoreboardService,
    private activatedRoute: ActivatedRoute,
    private claimsService: ClaimsService,
    private modalService: NgbModal,
    private userService: UserService,
  ) { }

  public ngOnInit() {
    this.activatedRoute.params.subscribe(params => {
      this.scoreboardService.apiScoreboardGet(params['contestId']).subscribe(scoreboard => {
        this.scoreboard = scoreboard.rows || [];
      });
    });

    this.userService.apiUsersGet().subscribe(user => {
      this.userId = user.id;
    });
  }

  private showSolvingAttemptSrc(entry: ScoreboardProblemEntryDto) {
    const modalRef = this.modalService.open(AttemptSrcViewModalComponent, { size: 'lg' });
    modalRef.componentInstance.attemptId = entry.solvingAttemptId;
    modalRef.componentInstance.contestId = this.activatedRoute.snapshot.params['contestId'];
  }

  public onCellClick(entry: ScoreboardProblemEntryDto, row: ScoreboardEntryDto) {
    if (entry.solvingAttemptId == Constants.EmptyGuid) return;

    if (row.userId == this.userId) {
      this.showSolvingAttemptSrc(entry);
      return;
    }

    let decided = false;
    this.claimsService.hasClaimObservable('ManageAttempts').subscribe(hasClaim => {
      if (hasClaim && !decided) {
        this.showSolvingAttemptSrc(entry);
        decided = true;
      }
    });

    this.claimsService.canAdjustContestGrade(this.activatedRoute.snapshot.params['contestId']).subscribe(canAdjust => {
      if (canAdjust && !decided) {
        this.showSolvingAttemptSrc(entry);
        decided = true;
      }
    });
  }

  protected readonly AttemptStatus = AttemptStatus;
}
