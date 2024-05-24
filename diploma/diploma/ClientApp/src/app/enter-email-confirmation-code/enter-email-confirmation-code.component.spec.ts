import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnterEmailConfirmationCodeComponent } from './enter-email-confirmation-code.component';

describe('EnterEmailConfirmationCodeComponent', () => {
  let component: EnterEmailConfirmationCodeComponent;
  let fixture: ComponentFixture<EnterEmailConfirmationCodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnterEmailConfirmationCodeComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EnterEmailConfirmationCodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
