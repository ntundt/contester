import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinishPasswordSignUpComponent } from './finish-password-sign-up.component';

describe('ConfirmSignUpComponent', () => {
  let component: FinishPasswordSignUpComponent;
  let fixture: ComponentFixture<FinishPasswordSignUpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinishPasswordSignUpComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(FinishPasswordSignUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
