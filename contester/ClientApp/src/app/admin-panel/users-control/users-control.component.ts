import {Component, OnInit} from '@angular/core';
import {NgFor, NgForOf, NgIf} from "@angular/common";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {TranslateModule} from "@ngx-translate/core";
import {AdminPanelUserDto, UserService} from "../../../generated/client";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {ToastsService} from "../../toasts/toasts.service";
import {PasswordResetModalComponent} from "../password-reset-modal/password-reset-modal.component";

@Component({
  selector: 'app-users-control',
  standalone: true,
  imports: [ NgFor, FormsModule, TranslateModule, NgIf ],
  templateUrl: './users-control.component.html',
  styleUrl: './users-control.component.css'
})
export class UsersControlComponent implements OnInit {
  protected users: AdminPanelUserDto[] = [];
  protected roles = ['Admin', 'User'];

  constructor(
    private userService: UserService,
    private modalService: NgbModal,
    private toastsService: ToastsService,
  ) { }

  changeRole(user: AdminPanelUserDto) {
    const role = user.role;
    this.userService.apiUsersUserIdRolePut(user.id!, { role }).subscribe(() => {
      this.toastsService.show({
        header: 'Role changed',
        body: `Role for user ${user.email} has been changed to ${role}`,
        type: 'success'
      });
    });
  }

  resetPassword(user: AdminPanelUserDto) {
    const passwordResetModal = this.modalService.open(PasswordResetModalComponent);
    passwordResetModal.result.then(password => {
      if (!password) return;
      this.userService.apiUsersUserIdPasswordPut(user.id!, { password }).subscribe(() => {
        this.toastsService.show({
          header: 'Password reset',
          body: `Password for user ${user.email} has been reset`,
          type: 'success'
        })
      });
    });
  }

  ngOnInit() {
    this.userService.apiUsersAllGet().subscribe(users => {
      this.users = users;
    });
  }
}
