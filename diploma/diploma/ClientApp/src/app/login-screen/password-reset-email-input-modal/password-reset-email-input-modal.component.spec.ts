import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PasswordResetEmailInputModalComponent } from './password-reset-email-input-modal.component';

describe('PasswordResetEmailInputModalComponent', () => {
  let component: PasswordResetEmailInputModalComponent;
  let fixture: ComponentFixture<PasswordResetEmailInputModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PasswordResetEmailInputModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PasswordResetEmailInputModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
