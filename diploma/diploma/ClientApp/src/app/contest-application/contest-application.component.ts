import { DatePipe, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthorizationService } from 'src/authorization/authorization.service';
import { ContestApplicationsService, ContestDto, ContestService } from 'src/generated/client';
import { ToastsService } from '../toasts/toasts.service';
import { TimerComponent } from "../shared/timer/timer.component";

@Component({
  selector: 'app-contest-application',
  standalone: true,
  templateUrl: './contest-application.component.html',
  styleUrl: './contest-application.component.css',
  imports: [DatePipe, NgIf, TimerComponent]
})
export class ContestApplicationComponent implements OnInit {
  public contest: ContestDto | undefined;
  public alreadyApplied: boolean = false;
  public isApplicationApproved: boolean = false;

  constructor(
    private contestApplicationService: ContestApplicationsService,
    private activatedRoute: ActivatedRoute,
    private contestService: ContestService,
    public authorizationService: AuthorizationService,
    private toastsService: ToastsService,
    private router: Router,
  ) { }

  ngOnInit() {
    const contestId = this.activatedRoute.snapshot.params.contestId;
    this.contestService.apiContestsGet(undefined, `id==${contestId}`).subscribe(res => {
      this.contest = res.contests?.[0];
    });
    this.contestApplicationService.apiContestApplicationsGet(contestId).subscribe(res => {
      this.alreadyApplied = res.alreadyApplied!;
      this.isApplicationApproved = res.isApplicationApproved!;
    });
  }
  
  apply() {
    const contestId = this.activatedRoute.snapshot.params.contestId;
    this.contestApplicationService.apiContestApplicationsPost({contestId}).subscribe(() => {
      this.toastsService.show({
        header: 'Success',
        body: 'You have successfully applied for the contest',
        type: 'success',
        delay: 5000,
      });
    });
  }

  onContestStarted() {
    this.router.navigate(['contest', this.contest?.id]);
  }
}
