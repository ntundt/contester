import { Component, OnInit } from '@angular/core';
import {faCheck, faClock, faCog, faDatabase, faListOl, faTasks, faUsers} from "@fortawesome/free-solid-svg-icons";
import {ActivatedRoute, Router} from "@angular/router";
import {ClaimsService} from "../../authorization/claims.service";
import {ContestDto, ContestService} from "../../generated/client";
import * as moment from 'moment';

interface SidebarItem {
  icon: any;
  text: string;
  route: string;
  requiresClaim?: string;
}

@Component({
  selector: 'app-main-area',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css'],
})
export class ContestComponent implements OnInit {
  public listItems: Array<SidebarItem> = [
    {icon: faDatabase, text: 'Schemas', route: 'schemas', requiresClaim: 'ManageSchemaDescriptions'},
    {icon: faTasks, text: 'Problems', route: 'problems'},
    {icon: faCheck, text: 'Attempts', route: 'attempts'},
    {icon: faUsers, text: 'Participants', route: 'participants', requiresClaim: 'ManageContestParticipants'},
    {icon: faListOl, text: 'Scoreboard', route: 'scoreboard'},
    {icon: faCog, text: 'Settings', route: 'settings', requiresClaim: 'ManageContests'}
  ];

  private contestId: string = '';

  public contest: ContestDto | undefined;

  constructor(private route: ActivatedRoute, public claimsService: ClaimsService,
              private contestsService: ContestService, private router: Router) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.contestId = params['contestId'];
    });
    this.contestsService.apiContestsGet().subscribe(res => {
      this.contest = res.contests?.find(contest => contest.id === this.route.snapshot.params['contestId']);
      this.refreshTimer();
    });

    setInterval(() => {
      this.checkContestEnded();
      this.refreshTimer();
    }, 1000);
  }

  public timeLeftString: string = '';

  private checkContestEnded() {
    if (this.contest?.finishDate && new Date(this.contest.finishDate).getTime() < Date.now() 
      && !this.claimsService.hasClaim('ManageContests')) {
      this.onContestEnded();
    }
  }

  onContestEnded() {
    this.router.navigate(['scoreboard', this.contestId]);
  }

  refreshTimer() {
    const totalRemainingSeconds = Math.floor((new Date(this.contest?.finishDate!).getTime() - Date.now()) / 1000);
    const hms = moment().startOf('day').seconds(totalRemainingSeconds).format('HH:mm:ss')
    if (totalRemainingSeconds < 24 * 60 * 60) {
      this.timeLeftString = hms;
    } else {
      const days = Math.floor(totalRemainingSeconds / (24 * 60 * 60));
      this.timeLeftString = `${days}d ${hms}`;
    }
  }

  public showTimer() {
    return new Date(this.contest?.startDate!).getTime() < Date.now() && new Date(this.contest?.finishDate!).getTime() > Date.now();
  }

  protected readonly faClock = faClock;
}
