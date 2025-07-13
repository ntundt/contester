import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { contestStartedGuard } from './contest-started.guard';

describe('contestStartedGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => contestStartedGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
