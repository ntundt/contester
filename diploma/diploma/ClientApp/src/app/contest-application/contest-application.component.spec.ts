import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContestApplicationComponent } from './contest-application.component';

describe('ContestApplicationComponent', () => {
  let component: ContestApplicationComponent;
  let fixture: ComponentFixture<ContestApplicationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContestApplicationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ContestApplicationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
