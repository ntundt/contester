import { Component, OnInit } from '@angular/core';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {AuthorizationService} from "../../authorization/authorization.service";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {AuthenticationService} from "../../generated/client";
import {
  PasswordResetEmailInputModalComponent
} from "./password-reset-email-input-modal/password-reset-email-input-modal.component";
import {ToastsService} from "../toasts/toasts.service";
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-screen',
  templateUrl: './login-screen.component.html',
  styleUrls: ['./login-screen.component.css']
})
export class LoginScreenComponent implements OnInit {
  constructor(
    private authenticationService: AuthorizationService,
    private authorizationService: AuthenticationService,
    private modalService: NgbModal,
    private toastsService: ToastsService,
    private router: Router,
  ) { }

  public formGroup: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
  });
  public waitingForResponse: boolean = false;

  public onSignIn(): void {
    const email: string = this.formGroup.get('email')?.value;
    const password: string = this.formGroup.get('password')?.value;

    if (this.formGroup.invalid) {
      return;
    }

    this.waitingForResponse = true;
    this.authenticationService.signIn(email, password).subscribe((res) => {
      this.waitingForResponse = false;
      this.router.navigate(['/']).then(() => {
        window.location.reload();
      });
    });
  }

  ngOnInit(): void {
  }

  private resetPassword(email: string): void {
    this.authorizationService.apiAuthRequestPasswordResetPost({ email }).subscribe((res) => {
      this.toastsService.show({
        header: 'Password reset',
        body: 'Check your email for further instructions',
        delay: 10000
      });
    });
  }

  public forgotPassword(): void {
    const modalRef = this.modalService.open(PasswordResetEmailInputModalComponent);
    modalRef.componentInstance.email = this.formGroup.get('email')?.value;
    modalRef.result.then(result => {
      if (result) {
        this.resetPassword(result);
      }
    });
  }
}
