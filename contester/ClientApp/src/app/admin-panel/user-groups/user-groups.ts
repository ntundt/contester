import {Component, OnInit} from '@angular/core';
import {TranslatePipe} from "@ngx-translate/core";
import {PrincipalDto, UserGroupService} from "../../../generated/client";
import {tap} from "rxjs/operators";
import {PrincipalCard} from "../../shared/principal-card/principal-card";
import {
  UserSelectionModalComponent
} from "../../main-area/settings/user-selection-modal/user-selection-modal.component";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {PrincipalSelectionModal} from "../../shared/principal-selection-modal/principal-selection-modal";

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

  loadingUserGroups: boolean = true;

  ngOnInit() {
    this.userGroupService.apiUserGroupSearchGet()
      .pipe(
        tap((data) => {
          this.loadingUserGroups = false;
          this.userGroups = data.data!;
          this.totalCount = 0;
        })
      ).subscribe()
  }
}
