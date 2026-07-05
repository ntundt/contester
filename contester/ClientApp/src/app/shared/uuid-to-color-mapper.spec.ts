import { TestBed } from '@angular/core/testing';

import { UuidToColorMapper } from './uuid-to-color-mapper';

describe('UuidToColorMapper', () => {
  let service: UuidToColorMapper;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UuidToColorMapper);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
