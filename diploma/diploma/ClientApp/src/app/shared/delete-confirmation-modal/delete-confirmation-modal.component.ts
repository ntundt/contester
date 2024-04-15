import {Component, Input} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-delete-confirmation-modal',
  standalone: true,
    imports: [
        FormsModule,
        TranslateModule,
    ],
  templateUrl: './delete-confirmation-modal.component.html',
  styleUrl: './delete-confirmation-modal.component.css'
})
export class DeleteConfirmationModalComponent {
  public constructor(public activeModal: NgbActiveModal) { }

  public closeModal() {
    this.activeModal.close(false);
  }

  public confirm() {
    this.activeModal.close(true);
  }
}
