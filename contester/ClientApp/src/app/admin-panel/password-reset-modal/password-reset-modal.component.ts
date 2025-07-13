import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-password-reset-modal',
  standalone: true,
  imports: [TranslateModule, FormsModule],
  templateUrl: './password-reset-modal.component.html',
  styleUrl: './password-reset-modal.component.css'
})
export class PasswordResetModalComponent {
  protected password: string = '';

  public constructor(public activeModal: NgbActiveModal) { }

  public closeModal() {
    this.activeModal.close();
  }

  public confirm() {
    this.activeModal.close(this.password);
  }
}
