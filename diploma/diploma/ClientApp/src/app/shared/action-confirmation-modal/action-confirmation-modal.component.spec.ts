import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActionConfirmationModalComponent } from './action-confirmation-modal.component';

describe('ActionConfirmationModalComponent', () => {
  let component: ActionConfirmationModalComponent;
  let fixture: ComponentFixture<ActionConfirmationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActionConfirmationModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ActionConfirmationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
