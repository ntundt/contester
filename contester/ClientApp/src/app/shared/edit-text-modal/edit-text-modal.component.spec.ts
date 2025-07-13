import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditTextModalComponent } from './edit-text-modal.component';

describe('EditTextModalComponent', () => {
  let component: EditTextModalComponent;
  let fixture: ComponentFixture<EditTextModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditTextModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditTextModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
