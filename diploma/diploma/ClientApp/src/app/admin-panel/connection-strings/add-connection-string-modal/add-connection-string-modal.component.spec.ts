import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddConnectionStringModalComponent } from './add-connection-string-modal.component';

describe('AddConnectionStringModalComponent', () => {
  let component: AddConnectionStringModalComponent;
  let fixture: ComponentFixture<AddConnectionStringModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddConnectionStringModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddConnectionStringModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
