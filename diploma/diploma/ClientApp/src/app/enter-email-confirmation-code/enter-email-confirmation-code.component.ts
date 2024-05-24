import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthenticationService } from 'src/generated/client';

@Component({
  selector: 'app-enter-email-confirmation-code',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule],
  templateUrl: './enter-email-confirmation-code.component.html',
  styleUrl: './enter-email-confirmation-code.component.css'
})
export class EnterEmailConfirmationCodeComponent {
  
  constructor(
    private authenticationService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  formGroup: FormGroup = new FormGroup({
    confirmationCode: new FormControl('', [Validators.required])
  });

  onSubmit() {
    const userId = this.route.snapshot.queryParamMap.get('userId')!;
    const code = this.formGroup.get('confirmationCode')?.value as string;
    this.authenticationService.apiAuthEmailConfirmationLinkGet(code, userId).subscribe(res => 
      {
        window.location.href = res.link!;
      });
  }

}
