import {Component, Input} from '@angular/core';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-action-confirmation-modal',
  standalone: true,
  imports: [],
  templateUrl: './action-confirmation-modal.component.html',
  styleUrl: './action-confirmation-modal.component.css'
})
export class ActionConfirmationModalComponent {
  @Input() public title: string = '';
  @Input() public message: string = '';
  @Input() public button: string = 'Confirm';

  public constructor(public activeModal: NgbActiveModal) { }

  public closeModal() {
    this.activeModal.close(false);
  }

  public confirm() {
    this.activeModal.close(true);
  }
}
