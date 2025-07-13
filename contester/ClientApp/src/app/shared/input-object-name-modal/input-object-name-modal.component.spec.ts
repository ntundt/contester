import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputObjectNameModalComponent } from './input-object-name-modal.component';

describe('InputObjectNameModalComponent', () => {
  let component: InputObjectNameModalComponent;
  let fixture: ComponentFixture<InputObjectNameModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputObjectNameModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InputObjectNameModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
