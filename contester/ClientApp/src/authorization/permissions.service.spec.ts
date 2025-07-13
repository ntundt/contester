import { TestBed } from '@angular/core/testing';

import { PermissionsService as PermissionsService } from './permissions.service';

describe('PermissionsService', () => {
  let service: PermissionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PermissionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
