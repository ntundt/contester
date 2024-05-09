import { NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { AdminPanelUserDto, UserService } from 'src/generated/client';
import { PasswordResetModalComponent } from './password-reset-modal/password-reset-modal.component';
import { ToastsService } from '../toasts/toasts.service';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [NgFor, FormsModule, TranslateModule, NgIf],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent implements OnInit {
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
