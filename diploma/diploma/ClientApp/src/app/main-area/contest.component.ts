import { Component, OnInit } from '@angular/core';
import {faCheck, faCog, faDatabase, faListOl, faTasks, faUsers} from "@fortawesome/free-solid-svg-icons";
import {ActivatedRoute} from "@angular/router";
import {ClaimsService} from "../../authorization/claims.service";

interface SidebarItem {
  icon: any;
  text: string;
  route: string;
  requiresClaim?: string;
}

@Component({
  selector: 'app-main-area',
  templateUrl: './contest.component.html',
  styleUrls: ['./contest.component.css']
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

  constructor(private route: ActivatedRoute, public claimsService: ClaimsService) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.contestId = params['contestId'];
    });
  }

}
