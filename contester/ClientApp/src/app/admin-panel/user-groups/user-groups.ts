import {Component, OnInit} from '@angular/core';
import {TranslatePipe} from "@ngx-translate/core";
import {PrincipalDto, UserGroupService} from "../../../generated/client";
import {tap} from "rxjs/operators";
import {PrincipalCard} from "../../shared/principal-card/principal-card";

@Component({
  selector: 'app-user-groups',
  imports: [
    TranslatePipe,
    PrincipalCard
  ],
  templateUrl: './user-groups.html',
  styleUrl: './user-groups.css',
})
export class UserGroups implements OnInit {
  public constructor(
    public userGroupService: UserGroupService,
  ) { }

  userGroups: Array<PrincipalDto>;
  totalCount: number = 0;

  ngOnInit() {
    this.userGroupService.apiUserGroupSearchGet()
      .pipe(
        tap((data) => {
          this.userGroups = data.data!;
          this.totalCount = 0;
        })
      ).subscribe()
  }
}
