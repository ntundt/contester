import {Component, Input} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-delete-confirmation-modal',
  standalone: true,
    imports: [
        FormsModule
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
