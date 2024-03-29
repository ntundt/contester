import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProblemAttemptsComponent } from './problem-attempts.component';

describe('ProblemAttemptsComponent', () => {
  let component: ProblemAttemptsComponent;
  let fixture: ComponentFixture<ProblemAttemptsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProblemAttemptsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ProblemAttemptsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
