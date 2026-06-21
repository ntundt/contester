import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrincipalSelectionModal } from './principal-selection-modal';

describe('PrincipalSelectionModal', () => {
  let component: PrincipalSelectionModal;
  let fixture: ComponentFixture<PrincipalSelectionModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrincipalSelectionModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrincipalSelectionModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
