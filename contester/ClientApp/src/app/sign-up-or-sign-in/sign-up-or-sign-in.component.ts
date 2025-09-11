import {Component} from '@angular/core';
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgIf} from "@angular/common";
import {TranslateModule} from "@ngx-translate/core";
import {AuthenticationService} from "../../generated/client";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {
  PrivacyPolicyConsentModalComponent
} from "./privacy-policy-consent-modal/privacy-policy-consent-modal.component";
import {Observable} from "rxjs";
import {faCheck, faXmark} from "@fortawesome/free-solid-svg-icons";
import {ToastsService} from "../toasts/toasts.service";
import {Router, RouterLink} from "@angular/router";
import {AuthenticationHelperService} from "../../authorization/authentication-helper.service";
import {faEnvelope} from "@fortawesome/free-regular-svg-icons";
import {
  PasswordResetConfirmationModalComponent
} from "./password-reset-confirmation-modal/password-reset-confirmation-modal.component";

@Component({
  selector: 'app-sign-up-or-sign-in',
  standalone: true,
  imports: [
    FaIconComponent,
    ReactiveFormsModule,
    NgIf,
    TranslateModule,
    RouterLink
  ],
  templateUrl: './sign-up-or-sign-in.component.html',
  styleUrl: './sign-up-or-sign-in.component.css'
})
export class SignUpOrSignInComponent {
  emailForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
  });

  passwordSignUpForm = new FormGroup({
    password: new FormControl('', [Validators.required]),
    passwordRepetition: new FormControl('', [Validators.required]),
  });

  passwordSignInForm = new FormGroup({
    password: new FormControl('', [Validators.required]),
  });

  flowState: 'stage_1' | 'stage_1_loading' | 'sign_up_enter_password' | 'sign_in_enter_password' | 'sign_in_check_email'
    | 'sign_up_check_email' | 'password_reset_check_email' = 'stage_1';

  constructor(
    private authenticationService: AuthenticationService,
    private authenticationHelperService: AuthenticationHelperService,
    private ngbModal: NgbModal,
    private toastsService: ToastsService,
    private router: Router
  ) { }

  protected formStage1IsValid() {
    return this.emailForm.valid;
  }

  public onBeginSignInOrSignUp() {
    this.flowState = 'stage_1_loading';
    const email = this.emailForm.controls.email.value!;
    this.authenticationService.apiAuthSignInOrSignUpByEmailGet(email).subscribe({
      next: (response) => {
        if (response.userFound) {
          if (response.authenticationMethod == 'Password') {
            this.passwordSignIn();
          } else if (response.authenticationMethod == 'EmailCode') {
            this.passwordlessSignIn();
          }
        } else {
          if (response.authenticationMethod == 'Password') {
            this.passwordSignUp();
          } else if (response.authenticationMethod == 'EmailCode') {
            this.passwordlessSignUp();
          }
        }
      }
    });
  }

  private privacyPolicyAgreement(): Observable<boolean> {
    const modalRef = this.ngbModal.open(PrivacyPolicyConsentModalComponent);
    return modalRef.closed;
  }

  public passwordSignIn() {
    this.flowState = 'sign_in_enter_password';
  }

  public passwordSignInFormIsValid() {
    return this.passwordSignInForm.valid;
  }

  public onPassowrdSignIn() {
    const email = this.emailForm.controls.email.value!;
    const password = this.passwordSignInForm.controls.password.value!;
    this.authenticationService.apiAuthPasswordSignInGet(email, password).subscribe(res => {
      this.authenticationHelperService.setCredentials(res);
      this.router.navigate(['/'])
        .then(() => window.location.reload());
    });
  }

  public passwordSignUp() {
    this.privacyPolicyAgreement().subscribe(res => {
      if (!res) {
        this.flowState = 'stage_1';
        return;
      }
      this.flowState = 'sign_up_enter_password';
    });
  }

  passwordIsLongEnough() {
    return this.passwordSignUpForm.controls.password.value!.length >= 8;
  }

  passwordContainsDigit() {
    return /[0-9]/.test(this.passwordSignUpForm.controls.password.value!);
  }

  passwordContainsUppercaseLetter() {
    return /[A-Z]/.test(this.passwordSignUpForm.controls.password.value!);
  }

  passwordContainsLowercaseLetter() {
    return /[a-z]/.test(this.passwordSignUpForm.controls.password.value!);
  }

  passwordsMatch() {
    return this.passwordSignUpForm.controls.password.value! === this.passwordSignUpForm.controls.passwordRepetition.value!;
  }

  passwordSignUpFormIsValid() {
    return this.passwordIsLongEnough() &&
      this.passwordContainsDigit() &&
      this.passwordContainsUppercaseLetter() &&
      this.passwordContainsLowercaseLetter() &&
      this.passwordsMatch();
  }

  onPasswordSignUp() {
    const email = this.emailForm.controls.email!.value!;
    const password = this.passwordSignUpForm.controls.password!.value!;
    this.authenticationService.apiAuthBeginPasswordSignUpPost({
      email,
      password,
    }).subscribe({
      next: (result) => {
        this.router.navigateByUrl('/enter-email-confirmation-code?userId=' + result.userId);
        this.toastsService.show({
          header: 'Success',
          body: 'Please check your inbox for further instructions',
          delay: 10000,
        });
      },
    });
  }


  public passwordlessSignIn() {
    const email = this.emailForm.controls.email.value!;
    this.authenticationService.apiAuthBeginPasswordlessSignInPost({email}).subscribe(() => {
      this.flowState = 'sign_in_check_email';
    });
  }

  public passwordlessSignUp() {
    this.privacyPolicyAgreement().subscribe(res => {
      if (!res) {
        this.flowState = 'stage_1';
        return;
      }
      const email = this.emailForm.controls.email.value!;
      this.authenticationService.apiAuthBeginPasswordlessSignUpGet(email).subscribe(() => {
        this.flowState = 'sign_up_check_email';
      });
    });
  }

  private resetPassword(email: string) {
    this.authenticationService.apiAuthRequestPasswordResetPost({ email }).subscribe(res => {
      this.flowState = 'password_reset_check_email';
    });
  }

  public forgotPassword(): void {
    const modalRef = this.ngbModal.open(PasswordResetConfirmationModalComponent);
    modalRef.componentInstance.email = this.emailForm.get('email')?.value;
    modalRef.result.then(result => {
      if (result) {
        this.resetPassword(result);
      }
    });
  }

  protected readonly faXmark = faXmark;
  protected readonly faCheck = faCheck;
  protected readonly faEnvelope = faEnvelope;
}
