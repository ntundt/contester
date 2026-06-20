import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrincipalCard } from './principal-card';

describe('PrincipalCard', () => {
  let component: PrincipalCard;
  let fixture: ComponentFixture<PrincipalCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrincipalCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrincipalCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
