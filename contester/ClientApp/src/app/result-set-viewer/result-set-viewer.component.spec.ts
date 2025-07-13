import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResultSetViewerComponent } from './result-set-viewer.component';

describe('ResultSetViewerComponent', () => {
  let component: ResultSetViewerComponent;
  let fixture: ComponentFixture<ResultSetViewerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResultSetViewerComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ResultSetViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
