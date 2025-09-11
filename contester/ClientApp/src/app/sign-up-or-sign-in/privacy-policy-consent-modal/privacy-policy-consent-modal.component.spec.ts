import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrivacyPolicyConsentModalComponent } from './privacy-policy-consent-modal.component';

describe('PrivacyPolicyConsentModalComponent', () => {
  let component: PrivacyPolicyConsentModalComponent;
  let fixture: ComponentFixture<PrivacyPolicyConsentModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrivacyPolicyConsentModalComponent]
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
