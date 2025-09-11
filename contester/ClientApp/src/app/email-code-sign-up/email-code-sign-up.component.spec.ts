import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailCodeSignUpComponent } from './email-code-sign-up.component';

describe('EmailCodeSignUpComponent', () => {
  let component: EmailCodeSignUpComponent;
  let fixture: ComponentFixture<EmailCodeSignUpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailCodeSignUpComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EmailCodeSignUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
