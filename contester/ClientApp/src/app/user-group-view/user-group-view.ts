import {Component, OnInit} from '@angular/core';
import {PrincipalDto, UserGroupService} from "../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {tap} from "rxjs/operators";
import {PrincipalCard} from "../shared/principal-card/principal-card";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {PrincipalSelectionModal} from "../shared/principal-selection-modal/principal-selection-modal";
import {DatePipe, UpperCasePipe} from "@angular/common";
import {InitialsPipe} from "../pipes/initials-pipe";
import {UuidToColorMapper} from "../shared/uuid-to-color-mapper";

@Component({
  selector: 'app-user-group-view',
  imports: [
    PrincipalCard,
    DatePipe,
    InitialsPipe,
    UpperCasePipe,
  ],
  templateUrl: './user-group-view.html',
  styleUrl: './user-group-view.css',
})
export class UserGroupView implements OnInit {
  constructor(
    private userGroupService: UserGroupService,
    private route: ActivatedRoute,
    private modalService: NgbModal,
  ) { }

  userGroup: PrincipalDto;
  loadingUserGroup: boolean = true;

  userGroupMembers: Array<PrincipalDto>;
  loadingUserGroupMembers: boolean = true;

  addPrincipalToGroup() {
    this.modalService.open(PrincipalSelectionModal)
      .result.then((selected: PrincipalDto) => {
        if (!selected) return;

        if (selected.type === 'User') {
          this.userGroupService
            .apiUserGroupGroupIdMemberUsersUserIdPost(selected.id!, this.userGroup.id!)
            .subscribe({
              next: () => this.refreshGroupMembers(),
              error: console.error,
            });
        } else if (selected.type === 'Group') {
          this.userGroupService
            .apiUserGroupParentGroupIdMemberGroupsChildGroupIdPost(this.userGroup.id!, selected.id!)
            .subscribe({
              next: () => this.refreshGroupMembers(),
              error: console.error,
            });
        }
      }
    );
  }

  removeMemberFromGroup(memberType: string, memberId: string) {
    if (memberType === 'User') {
      this.userGroupService.apiUserGroupGroupIdMemberUsersUserIdDelete(memberId, this.userGroup.id!)
        .subscribe({
          next: () => this.refreshGroupMembers(),
          error: (err) => {
            console.error(err);
          },
        });
    } else if (memberType === 'Group') {
      this.userGroupService.apiUserGroupParentGroupIdMemberGroupsChildGroupIdDelete(this.userGroup.id!, memberId)
        .subscribe({
          next: () => this.refreshGroupMembers(),
          error: (err) => {
            console.error(err);
          },
        });
    }
  }

  refreshGroupMembers() {
    const groupId = this.route.snapshot.params['groupId'];

    this.loadingUserGroupMembers = true;
    this.userGroupService.apiUserGroupParentGroupIdMembersGet(groupId)
      .pipe(
        tap(res => {
          this.userGroupMembers = res;
          this.loadingUserGroupMembers = false;
        })
      ).subscribe();
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      const groupId = params['groupId'];

      this.loadingUserGroup = true;
      this.userGroupService.apiUserGroupSearchGet(`Id==${groupId}`)
        .pipe(
          tap(res => {
            this.userGroup = res.data![0];
            this.loadingUserGroup = false;
          }),
        ).subscribe();

      this.refreshGroupMembers();
    });
  }

  protected readonly UuidToColorMapper = UuidToColorMapper;
}
