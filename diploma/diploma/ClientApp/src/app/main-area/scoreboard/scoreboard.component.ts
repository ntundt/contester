import {Component, OnInit} from '@angular/core';
import {AttemptStatus, ScoreboardEntryDto, ScoreboardService} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {NgForOf} from "@angular/common";
import {faCheck, faTimes} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [NgForOf, FaIconComponent],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.css'
})
export class ScoreboardComponent implements OnInit {
  public scoreboard: Array<ScoreboardEntryDto> = [];

  public faCheck = faCheck;
  public faTimes = faTimes;

  public constructor(private scoreboardService: ScoreboardService, private activatedRoute: ActivatedRoute) { }

  public ngOnInit() {
    this.activatedRoute.params.subscribe(params => {
      this.scoreboardService.apiScoreboardGet(params['contestId']).subscribe(scoreboard => {
        this.scoreboard = scoreboard.rows || [];
      });
    });
  }

  protected readonly AttemptStatus = AttemptStatus;
}
