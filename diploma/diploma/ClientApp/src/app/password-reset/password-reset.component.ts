import { Component } from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ActivatedRoute, Router} from "@angular/router";
import {AuthenticationService} from "../../generated/client";

@Component({
  selector: 'app-password-reset',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './password-reset.component.html',
  styleUrl: './password-reset.component.css'
})
export class PasswordResetComponent {
  public password: string = '';
  public confirmPassword: string = '';

  public constructor(private route: ActivatedRoute, private authenticationService: AuthenticationService, private router: Router) { }

  public confirm(): void {
    let token = this.route.snapshot.queryParamMap.get('token');
    this.authenticationService.apiAuthResetPasswordPost({ passwordResetToken: token!, newPassword: this.password }).subscribe({
      next: (res) => {
        this.router.navigate(['/login']);
      },
      error: (err) => {
        if (err.error.err === 103) { // invalid token
          alert('Invalid token');
        } else if (err.error.err === 104) { // token expired
          alert('Token expired');
        }
      }
    });
  }

  private passwordFitsConstraints(): boolean {
    let hasUpperCase = this.password.toLowerCase() !== this.password;
    let hasLowerCase = this.password.toUpperCase() !== this.password;
    let hasDigit = /\d/.test(this.password);
    return this.password.length >= 8 && hasUpperCase && hasLowerCase && hasDigit;
  }

  public passwordsAreOk(): boolean {
    return this.password === this.confirmPassword && this.passwordFitsConstraints();
  }
}
