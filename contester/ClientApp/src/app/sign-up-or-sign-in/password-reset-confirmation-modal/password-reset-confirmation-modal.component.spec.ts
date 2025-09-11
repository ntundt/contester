import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PasswordResetConfirmationModalComponent } from './password-reset-confirmation-modal.component';

describe('PasswordResetConfirmationModalComponent', () => {
  let component: PasswordResetConfirmationModalComponent;
  let fixture: ComponentFixture<PasswordResetConfirmationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PasswordResetConfirmationModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PasswordResetConfirmationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
