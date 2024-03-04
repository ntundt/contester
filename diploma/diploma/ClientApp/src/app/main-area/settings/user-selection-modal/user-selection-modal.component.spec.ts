import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserSelectionModalComponent } from './user-selection-modal.component';

describe('UserSelectionModalComponent', () => {
  let component: UserSelectionModalComponent;
  let fixture: ComponentFixture<UserSelectionModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserSelectionModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(UserSelectionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
