import { Component } from '@angular/core';
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ActivatedRoute, Router} from "@angular/router";
import {AuthorizationService} from "../../authorization/authorization.service";

@Component({
  selector: 'app-confirm-sign-up',
  standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule
    ],
  templateUrl: './confirm-sign-up.component.html',
  styleUrl: './confirm-sign-up.component.css'
})
export class ConfirmSignUpComponent {
  public formGroup: FormGroup = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    patronymic: new FormControl('', []),
    additionalInfo: new FormControl('', [Validators.required]),
  });

  public constructor(private route: ActivatedRoute, private router: Router, private authenticationService: AuthorizationService) { }

  public onConfirmSignUp(): void {
    const firstName: string = this.formGroup.get('firstName')?.value;
    const lastName: string = this.formGroup.get('lastName')?.value;
    const patronymic: string = this.formGroup.get('patronymic')?.value;
    const additionalInfo: string = this.formGroup.get('additionalInfo')?.value;
    const token = this.route.snapshot.queryParams['token'];

    if (!token) {
      alert('Invalid token');
      return;
    }

    if (this.formGroup.invalid) {
      return;
    }

    this.authenticationService.confirmSignUp({firstName, lastName, patronymic, additionalInfo, token}).subscribe({
      next: (res) => {
        this.router.navigate(['/']);
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
