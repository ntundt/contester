import { Component, Input } from '@angular/core';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {FormsModule} from "@angular/forms";
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-password-reset-email-input-modal',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule,
  ],
  templateUrl: './password-reset-email-input-modal.component.html',
  styleUrl: './password-reset-email-input-modal.component.css'
})
export class PasswordResetEmailInputModalComponent {
  @Input() public email: string = '';

  public constructor(private activeModal: NgbActiveModal) { }

  public confirm(): void {
    this.activeModal.close(this.email);
  }

  public cancel(): void {
    this.activeModal.dismiss();
  }
}
