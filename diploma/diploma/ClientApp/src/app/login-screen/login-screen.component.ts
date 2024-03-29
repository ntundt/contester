import { Component, OnInit } from '@angular/core';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {AuthorizationService} from "../../authorization/authorization.service";
import {Observable} from "rxjs";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {
  ActionConfirmationModalComponent
} from "../shared/action-confirmation-modal/action-confirmation-modal.component";
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

  private beginSignUp(): void {
    const email: string = this.formGroup.get('email')?.value;
    const password: string = this.formGroup.get('password')?.value;

    this.waitingForResponse = true;
    this.authenticationService.beginSignUp({ email, password }).subscribe({
      next: (res) => {
        this.waitingForResponse = false;
        this.toastsService.show({
          header: 'Success',
          body: 'Check your email for further instructions',
          delay: 5000,
        });
      },
      error: (err) => {
        this.waitingForResponse = false;
        if (err.error.err === 101) { // user already exists
          this.toastsService.show({
            header: 'Error',
            body: 'User already exists',
            delay: 5000,
            type: 'error'
          });
        }
      }
    });
  }

  public confirmBeginSignUp(): void {
    const modalRef = this.modalService.open(ActionConfirmationModalComponent);
    modalRef.componentInstance.title = 'User not found';
    modalRef.componentInstance.message = 'Do you want to create a new account?';
    modalRef.componentInstance.button = 'Create';
    modalRef.result.then(result => {
      if (!result) return;
      this.beginSignUp();
    });
  }

  public onSignIn(): void {
    const email: string = this.formGroup.get('email')?.value;
    const password: string = this.formGroup.get('password')?.value;

    if (this.formGroup.invalid) {
      return;
    }

    this.waitingForResponse = true;
    this.authenticationService.signIn(email, password).subscribe({
      next: (res) => {
        this.waitingForResponse = false;
        this.router.navigate(['/']).then(() => {
          window.location.reload();
        });
      },
      error: (err) => {
        this.waitingForResponse = false;
        if (err.error.err === 104) {
          this.toastsService.show({
            header: 'Error',
            body: 'Wrong password',
            delay: 5000,
            type: 'error'
          });
        } else if (err.error.err === 102) { // user not found
          this.confirmBeginSignUp();
        } else if (err.error.err === 109) {
          this.toastsService.show({
            header: 'Error',
            body: 'Email is not verified. Please check your email for further instructions',
            delay: 5000,
            type: 'error'
          });
        }
      }
    });
  }

  ngOnInit(): void {
  }

  public forgotPassword(): void {
    const modalRef = this.modalService.open(PasswordResetEmailInputModalComponent);
    modalRef.result.then(result => {
      if (result) {
        this.authorizationService.apiAuthRequestPasswordResetPost({ email: result }).subscribe({
          next: (res) => {
            this.toastsService.show({
              header: 'Password reset',
              body: 'Check your email for further instructions',
              delay: 5000
            });
          },
          error: (err) => {
            if (err.error.err === 102) {
              this.toastsService.show({
                header: 'Error',
                body: 'User not found',
                delay: 5000,
                type: 'error'
              });
            }
          }
        });
      }
    });
  }
}
