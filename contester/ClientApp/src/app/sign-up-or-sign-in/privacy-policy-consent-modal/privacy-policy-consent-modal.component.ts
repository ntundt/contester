import {Component, OnInit} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {TranslateModule} from "@ngx-translate/core";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {MarkdownComponent} from "ngx-markdown";
import {ApplicationSettingsService} from "../../../generated/client";

@Component({
  selector: 'app-privacy-policy-consent-modal',
  standalone: true,
    imports: [
        MarkdownComponent,
        FormsModule,
        TranslateModule
    ],
  templateUrl: './privacy-policy-consent-modal.component.html',
  styleUrl: './privacy-policy-consent-modal.component.css'
})
export class PrivacyPolicyConsentModalComponent implements OnInit {
  constructor(
    private activeModal: NgbActiveModal,
    private applicationSettingsService: ApplicationSettingsService,
  ) { }

  public privacyPolicyConsent: boolean = false;
  public privacyPolicyConsentRequired: boolean = true;
  public privacyPolicy: string = '';
  public loading: boolean = true;

  ngOnInit(): void {
    this.applicationSettingsService.apiApplicationSettingsPrivacyPolicyGet().subscribe({
      next: (response) => {
        this.privacyPolicy = response.privacyPolicy ?? '';
        this.privacyPolicyConsentRequired = response.privacyPolicyConsentRequired ?? true;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  decline() {
    this.activeModal.close(false);
  }

  accept() {
    this.activeModal.close(true);
  }
}
