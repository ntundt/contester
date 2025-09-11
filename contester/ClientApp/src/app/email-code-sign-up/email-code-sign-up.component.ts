import { Component } from '@angular/core';
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {TranslateModule} from "@ngx-translate/core";
import {ActivatedRoute, Router} from "@angular/router";
import {AuthenticationHelperService} from "../../authorization/authentication-helper.service";
import {AuthenticationService} from "../../generated/client";

@Component({
  selector: 'app-email-code-sign-up',
  standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        TranslateModule
    ],
  templateUrl: './email-code-sign-up.component.html',
  styleUrl: './email-code-sign-up.component.css'
})
export class EmailCodeSignUpComponent {
  public personalInfo: FormGroup = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    patronymic: new FormControl('', []),
    additionalInfo: new FormControl('', [Validators.required]),
  });

  public constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authenticationHelperService: AuthenticationHelperService,
    private authenticationService: AuthenticationService,
  ) { }

  public onConfirmSignUp(): void {
    const firstName: string = this.personalInfo.get('firstName')?.value;
    const lastName: string = this.personalInfo.get('lastName')?.value;
    const patronymic: string = this.personalInfo.get('patronymic')?.value;
    const additionalInfo: string = this.personalInfo.get('additionalInfo')?.value;
    const emailCode = this.route.snapshot.queryParams['emailCode'];
    const email = this.route.snapshot.queryParams['email']

    if (!emailCode) {
      alert('Invalid token');
      return;
    }

    if (this.personalInfo.invalid) {
      return;
    }

    this.authenticationService.apiAuthFinishPasswordlessSignUpPost({firstName, lastName, patronymic, additionalInfo, emailCode, email}).subscribe({
      next: (res) => {
        this.authenticationHelperService.setCredentials(res);
        this.router.navigate(['/'])
          .then(() => window.location.reload());
      },
      error: (err) => {
        if (err.error.err === 102) { // user not found
          alert('Invalid token');
        } else if (err.error.err === 110) {
          alert('Email already confirmed');
        }
      }
    });
  }
}
