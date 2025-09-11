import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SignUpOrSignInComponent } from './sign-up-or-sign-in.component';

describe('SignUpOrSignInScreenComponent', () => {
  let component: SignUpOrSignInComponent;
  let fixture: ComponentFixture<SignUpOrSignInComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SignUpOrSignInComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SignUpOrSignInComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
