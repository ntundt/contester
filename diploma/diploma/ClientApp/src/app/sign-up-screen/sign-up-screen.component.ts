import { Component, OnInit } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheck, faCross, faXmark } from '@fortawesome/free-solid-svg-icons';
import { AuthenticationService } from 'src/generated/client';
import { ToastsService } from '../toasts/toasts.service';
import { NgIf } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-sign-up-screen',
  standalone: true,
  imports: [
    FaIconComponent,
    NgIf,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
  ],
  templateUrl: './sign-up-screen.component.html',
  styleUrl: './sign-up-screen.component.css'
})
export class SignUpScreenComponent implements OnInit {
  public email: string = '';
  public password: string = '';
  public passwordConfirmation: string = '';

  constructor(
    private authenticationService: AuthenticationService,
    private toastsService: ToastsService,
  ) { }

  ngOnInit(): void {

  }

  passwordIsLongEnough() {
    return this.password.length >= 8;
  }

  passwordContainsDigit() {
    return /[0-9]/.test(this.password);
  }

  passwordContainsUppercaseLetter() {
    return /[A-Z]/.test(this.password);
  }

  passwordContainsLowercaseLetter() {
    return /[a-z]/.test(this.password);
  }

  passwordsMatch() {
    return this.password === this.passwordConfirmation;
  }

  formIsValid() {
    return this.passwordIsLongEnough() &&
      this.passwordContainsDigit() &&
      this.passwordContainsUppercaseLetter() &&
      this.passwordContainsLowercaseLetter() &&
      this.passwordsMatch();
  }

  onSignUp() {
    this.authenticationService.apiAuthBeginSignUpPost({
      email: this.email,
      password: this.password,
    }).subscribe({
      next: () => {
        this.toastsService.show({
          header: 'Success',
          body: 'Please check your email for further instructions',
          delay: 10000,
        });
      },
    });
  }

  protected readonly faXmark = faXmark;
  protected readonly faCheck = faCheck;
}
