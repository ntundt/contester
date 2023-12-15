import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AttemptSrcViewModalComponent } from './attempt-src-view-modal.component';

describe('AttemptSrcViewModalComponent', () => {
  let component: AttemptSrcViewModalComponent;
  let fixture: ComponentFixture<AttemptSrcViewModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AttemptSrcViewModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AttemptSrcViewModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
