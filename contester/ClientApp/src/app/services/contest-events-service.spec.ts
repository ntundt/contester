import { TestBed } from '@angular/core/testing';

import { ContestEventsService } from './contest-events-service';

describe('ContestEventsService', () => {
  let service: ContestEventsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ContestEventsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
