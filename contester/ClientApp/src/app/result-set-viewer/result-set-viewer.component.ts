import {Component, OnInit} from '@angular/core';
import {AttemptService, ResultSet} from "../../generated/client";
import { TranslateModule } from "@ngx-translate/core";
import { ActivatedRoute } from "@angular/router";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-result-set-viewer',
  standalone: true,
  imports: [
    TranslateModule,
    NgForOf,
  ],
  templateUrl: './result-set-viewer.component.html',
  styleUrl: './result-set-viewer.component.css'
})
export class ResultSetViewerComponent implements OnInit {
  public constructor(
    private attemptService: AttemptService,
    private activatedRoute: ActivatedRoute,
  ) { }

  public expectedResultSet: ResultSet | undefined;
  public actualResultSet: ResultSet | undefined;

  ngOnInit() {
    this.activatedRoute.params.subscribe(params => {
      const attemptId = params['attemptId'];
      if (!attemptId) return;
      this.attemptService.apiAttemptsAttemptIdResultSetGet(attemptId).subscribe(resultSets => {
        this.expectedResultSet = resultSets.expectedResult;
        this.actualResultSet = resultSets.actualResult;
      });
    });
  }
}
