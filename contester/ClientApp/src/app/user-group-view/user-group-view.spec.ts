import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserGroupView } from './user-group-view';

describe('UserGroupView', () => {
  let component: UserGroupView;
  let fixture: ComponentFixture<UserGroupView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserGroupView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserGroupView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
