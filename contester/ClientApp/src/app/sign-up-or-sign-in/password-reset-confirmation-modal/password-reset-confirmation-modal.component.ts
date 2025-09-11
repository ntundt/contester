import {Component, Input} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {TranslateModule} from "@ngx-translate/core";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-password-reset-confirmation-modal',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule
  ],
  templateUrl: './password-reset-confirmation-modal.component.html',
  styleUrl: './password-reset-confirmation-modal.component.css'
})
export class PasswordResetConfirmationModalComponent {
  @Input() public email: string = '';

  public constructor(private activeModal: NgbActiveModal) { }

  public confirm(): void {
    this.activeModal.close(this.email);
  }

  public cancel(): void {
    this.activeModal.dismiss();
  }
}
