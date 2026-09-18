import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrivacyPolicyConsentModalComponent } from './privacy-policy-consent-modal.component';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {ApplicationSettingsService} from "../../../generated/client";
import {of} from "rxjs";

describe('PrivacyPolicyConsentModalComponent', () => {
  let component: PrivacyPolicyConsentModalComponent;
  let fixture: ComponentFixture<PrivacyPolicyConsentModalComponent>;

  const applicationSettingsServiceMock = {
    apiApplicationSettingsPrivacyPolicyGet: () => of({privacyPolicy: '# Privacy policy', privacyPolicyConsentRequired: true}),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrivacyPolicyConsentModalComponent],
      providers: [
        NgbActiveModal,
        {provide: ApplicationSettingsService, useValue: applicationSettingsServiceMock},
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrivacyPolicyConsentModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
