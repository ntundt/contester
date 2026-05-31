import { Component } from '@angular/core';
import {CodeEditorModule} from "@ngstack/code-editor";
import {FormsModule} from "@angular/forms";
import {TranslateModule} from "@ngx-translate/core";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-privacy-policy-consent-modal',
  standalone: true,
    imports: [
        CodeEditorModule,
        FormsModule,
        TranslateModule
    ],
  templateUrl: './privacy-policy-consent-modal.component.html',
  styleUrl: './privacy-policy-consent-modal.component.css'
})
export class PrivacyPolicyConsentModalComponent {
  constructor(
    private activeModal: NgbActiveModal,
  ) { }

  public privacyPolicyConsent: boolean = false;

  decline() {
    this.activeModal.close(false);
  }

  accept() {
    this.activeModal.close(true);
  }
}
