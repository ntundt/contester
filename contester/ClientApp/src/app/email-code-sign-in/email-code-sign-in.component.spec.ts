import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailCodeSignInComponent } from './email-code-sign-in.component';

describe('EmailCodeSignInComponent', () => {
  let component: EmailCodeSignInComponent;
  let fixture: ComponentFixture<EmailCodeSignInComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailCodeSignInComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EmailCodeSignInComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
