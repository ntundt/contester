import { Component } from '@angular/core';
import {AuthorizationService} from "../../../authorization/authorization.service";
import {Router, RouterLink} from "@angular/router";
import {UserService} from "../../../generated/client";
import {NgIf} from "@angular/common";
import {NgbDropdown, NgbDropdownMenu, NgbDropdownToggle} from "@ng-bootstrap/ng-bootstrap";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {faArrowRightFromBracket, faSignInAlt, faUser} from "@fortawesome/free-solid-svg-icons";

@Component({
  selector: 'app-account-control',
  standalone: true,
  imports: [
    NgIf,
    NgbDropdownToggle,
    RouterLink,
    NgbDropdownMenu,
    NgbDropdown,
    FaIconComponent
  ],
  templateUrl: './account-control.component.html',
  styleUrl: './account-control.component.css'
})
export class AccountControlComponent {
  public email: string | undefined = '';

  public constructor(public authorizationService: AuthorizationService, public router: Router, private userService: UserService) {
    this.userService.apiUsersGet().subscribe(res => {
      this.email = res.email;
    });
  }

  public logout() {
    this.authorizationService.signOut();
    this.router.navigate(['/login']);
  }

  protected readonly faArrowRightFromBracket = faArrowRightFromBracket;
  protected readonly faUser = faUser;
  protected readonly faSignInAlt = faSignInAlt;
}
