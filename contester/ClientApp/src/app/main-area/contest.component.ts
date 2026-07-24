import { Component, OnInit } from '@angular/core';
import {faCheck, faClock, faCog, faDatabase, faListOl, faTasks, faUsers} from "@fortawesome/free-solid-svg-icons";
import {ActivatedRoute, Router} from "@angular/router";
import {PermissionsService} from "../../authorization/permissions.service";
import {ContestDto, ContestService, ProblemDto, ProblemService} from "../../generated/client";
import { TranslateService } from '@ngx-translate/core';
import {ContestEventsService} from "../services/contest-events-service";

interface SidebarItem {
  id: number;
  icon: any;
  text: string;
  route: string;
  requiresPermission?: string;
  activeExactMatch: boolean;
}

@Component({
  selector: 'app-main-area',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css'],
  standalone: false,
})
export class ContestComponent implements OnInit {
  public listItems: Array<SidebarItem> = [
    {id: 1, icon: faDatabase, text: 'sidebar.schemas', route: 'schemas', requiresPermission: 'ManageSchemaDescriptions', activeExactMatch: false},
    {id: 2, icon: faTasks, text: 'sidebar.problems', route: 'problems', activeExactMatch: true},
    {id: 3, icon: faCheck, text: 'sidebar.attempts', route: 'attempts', requiresPermission: 'ManageAttempts', activeExactMatch: false},
    {id: 4, icon: faUsers, text: 'sidebar.participants', route: 'participants', requiresPermission: 'ManageContestParticipants', activeExactMatch: false},
    {id: 5, icon: faListOl, text: 'sidebar.scoreboard', route: 'scoreboard', activeExactMatch: false},
    {id: 6, icon: faCog, text: 'sidebar.settings', route: 'settings', requiresPermission: 'ManageContests', activeExactMatch: false},
  ];

  public contest: ContestDto | undefined;

  contestGoingOnUntil: Date = new Date();

  problems: Array<ProblemDto> = [];

  constructor(
    private route: ActivatedRoute,
    public permissionsService: PermissionsService,
    private contestsService: ContestService,
    private router: Router,
    private translateService: TranslateService,
    private problemService: ProblemService,
    private events: ContestEventsService,
  ) {
    this.onContestEnded = this.onContestEnded.bind(this);
  }

  refreshProblems(contestId: string) {
    this.problemService.apiProblemsGet(contestId).subscribe(res => {
      this.problems = res.problems!;
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const contestId = params['contestId'];

      this.contestsService.apiContestsGet().subscribe(res => {
        this.contest = res.contests?.find(contest => contest.id === contestId);
        this.contestGoingOnUntil = new Date(Date.now() + (this.contest?.timeUntilFinishSeconds ?? 0) * 1000);
      });

      this.refreshProblems(contestId);
    });

    this.events.problemsChanged$.subscribe(() => {
      const contestId = this.route.snapshot.params['contestId'];
      this.refreshProblems(contestId);
    });
  }

  onContestEnded() {
    if (!this.permissionsService.hasPermission('ManageContests')) {
      const contestId = this.route.snapshot.params['contestId'];
      this.router.navigate(['/scoreboard', contestId]);
    }
  }

  public showTimer() {
    return new Date(this.contest?.startDate!).getTime() < Date.now()
      && new Date(this.contest?.finishDate!).getTime() > Date.now();
  }

  protected readonly faClock = faClock;
}
