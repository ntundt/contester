import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountControlComponent } from './account-control.component';

describe('AccountControlComponent', () => {
  let component: AccountControlComponent;
  let fixture: ComponentFixture<AccountControlComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountControlComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AccountControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
