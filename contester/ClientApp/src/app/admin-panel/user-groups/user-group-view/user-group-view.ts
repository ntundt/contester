import {Component, OnInit} from '@angular/core';
import {PrincipalDto, UserGroupService} from "../../../../generated/client";
import {ActivatedRoute, Router} from "@angular/router";
import {tap} from "rxjs/operators";
import {PrincipalCard} from "../../../shared/principal-card/principal-card";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {PrincipalSelectionModal} from "../../../shared/principal-selection-modal/principal-selection-modal";
import {DatePipe, UpperCasePipe} from "@angular/common";
import {InitialsPipe} from "../../../pipes/initials-pipe";
import {UuidToColorMapper} from "../../../shared/uuid-to-color-mapper";
import {
  DeleteConfirmationModalComponent
} from "../../../shared/delete-confirmation-modal/delete-confirmation-modal.component";
import {TranslatePipe, TranslateService} from "@ngx-translate/core";
import {DeclensionPipe} from "../../../pipes/declension-pipe";
import {faPencil} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {RenameModal} from "../../../shared/rename-modal/rename-modal";

@Component({
  selector: 'app-user-group-view',
  imports: [
    PrincipalCard,
    DatePipe,
    InitialsPipe,
    UpperCasePipe,
    TranslatePipe,
    DeclensionPipe,
    FaIconComponent,
  ],
  templateUrl: './user-group-view.html',
  styleUrl: './user-group-view.css',
})
export class UserGroupView implements OnInit {
  constructor(
    private userGroupService: UserGroupService,
    private route: ActivatedRoute,
    private router: Router,
    private modalService: NgbModal,
    private translateService: TranslateService,
  ) { }

  userGroup: PrincipalDto;
  loadingUserGroup: boolean = true;

  userGroupMembers: Array<PrincipalDto>;
  loadingUserGroupMembers: boolean = true;

  deleteUserGroup() {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      this.userGroupService.apiUserGroupGroupIdDelete(this.userGroup.id!)
        .subscribe(() => {
          this.router.navigate(['/admin-panel/user-groups']);
        });
    });
  }

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

  refreshGroupInfo() {
    const groupId = this.route.snapshot.params['groupId'];

    this.loadingUserGroup = true;
    this.userGroupService.apiUserGroupSearchGet(`Id==${groupId}`)
      .pipe(
        tap(res => {
          this.userGroup = res.data![0];
          this.loadingUserGroup = false;
        }),
      ).subscribe();
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.refreshGroupInfo();
      this.refreshGroupMembers();
    });
  }

  renameUserGroup() {
    const modalRef = this.modalService.open(RenameModal, {
      backdrop: 'static',
    });

    modalRef.componentInstance.name = this.userGroup.displayName;
    modalRef.componentInstance.title = this.translateService.instant('group.rename');

    modalRef.result
      .then((result: string | undefined) => {
        if (!!result) {
          this.userGroupService.apiUserGroupGroupIdNamePut(this.userGroup.id!, result)
            .subscribe(() => {
              this.refreshGroupInfo();
            });
        }
      })
      .catch(() => { });
  }

  protected readonly UuidToColorMapper = UuidToColorMapper;
  protected readonly faPencil = faPencil;
}
