import { TestBed } from '@angular/core/testing';

import { ScoreboardUpdatesService } from './scoreboard-updates.service';

describe('ScoreboardUpdatesService', () => {
  let service: ScoreboardUpdatesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScoreboardUpdatesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
