import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectionStringComponent } from './connection-string.component';

describe('ConnectionStringComponent', () => {
  let component: ConnectionStringComponent;
  let fixture: ComponentFixture<ConnectionStringComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConnectionStringComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ConnectionStringComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
