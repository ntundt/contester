import { Component, OnInit } from '@angular/core';
import { ScoreboardComponent } from '../main-area/scoreboard/scoreboard.component';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { ClaimsService } from '../../authorization/claims.service';


@Component({
  selector: 'app-final-scoreboard',
  standalone: true,
  imports: [ScoreboardComponent, RouterLink, NgIf],
  templateUrl: './final-scoreboard.component.html',
  styleUrl: './final-scoreboard.component.css'
})
export class FinalScoreboardComponent implements OnInit {
  public hasManageContestsClaim: boolean = false;
  
  constructor(
    public activatedRoute: ActivatedRoute,
    public claimsService: ClaimsService
  ) { }

  public ngOnInit() {
    this.activatedRoute.snapshot.params['contestId'];
    this.claimsService.hasClaimObservable('ManageContests').subscribe(res => {
      this.hasManageContestsClaim = res;
    });
  }
}
