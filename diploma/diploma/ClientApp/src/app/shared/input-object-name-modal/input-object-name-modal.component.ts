import {Component, Input} from '@angular/core';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-input-object-name-modal',
  standalone: true,
  imports: [
    FormsModule
  ],
  templateUrl: './input-object-name-modal.component.html',
  styleUrl: './input-object-name-modal.component.css'
})
export class InputObjectNameModalComponent {
  public name: string = '';

  @Input() title: string = '';

  public constructor(public activeModal: NgbActiveModal) { }

  public closeModal() {
    this.activeModal.close();
  }

  public confirm() {
    this.activeModal.close(this.name);
  }
}
