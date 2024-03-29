import { Component, OnInit } from '@angular/core';
import { ScoreboardComponent } from '../main-area/scoreboard/scoreboard.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { ClaimsService } from '../../authorization/claims.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { ContestService } from 'src/generated/client';
import { faFileArrowDown } from '@fortawesome/free-solid-svg-icons';


@Component({
  selector: 'app-final-scoreboard',
  standalone: true,
  imports: [ScoreboardComponent, RouterLink, NgIf, FaIconComponent],
  templateUrl: './final-scoreboard.component.html',
  styleUrl: './final-scoreboard.component.css'
})
export class FinalScoreboardComponent implements OnInit {
  public hasManageContestsClaim: boolean = false;
  public canAdjustContestGrades: boolean = false;
  
  constructor(
    public activatedRoute: ActivatedRoute,
    public claimsService: ClaimsService,
    public contestService: ContestService,
  ) { }

  public ngOnInit() {
    this.activatedRoute.snapshot.params['contestId'];
    this.claimsService.hasClaimObservable('ManageContests').subscribe(res => {
      this.hasManageContestsClaim = res;
    });
    const contestId = this.activatedRoute.snapshot.params['contestId'];
    this.claimsService.canAdjustContestGrade(contestId).subscribe(res => {
      this.canAdjustContestGrades = res;
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

  protected readonly faFileArrowDown = faFileArrowDown;
}
