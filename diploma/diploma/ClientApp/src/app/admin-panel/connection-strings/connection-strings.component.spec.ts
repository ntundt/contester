import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectionStringsComponent } from './connection-strings.component';

describe('ConnectionStringsComponent', () => {
  let component: ConnectionStringsComponent;
  let fixture: ComponentFixture<ConnectionStringsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConnectionStringsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ConnectionStringsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
