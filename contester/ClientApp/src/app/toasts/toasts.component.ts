import { Component } from '@angular/core';
import {NgbToast} from "@ng-bootstrap/ng-bootstrap";
import {ToastsService} from "./toasts.service";
@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [
    NgbToast,
  ],
  templateUrl: './toasts.component.html',
  styleUrl: './toasts.component.css'
})
export class ToastsComponent {

  public constructor(public toastsService: ToastsService) { }

}
