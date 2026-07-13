import { TestBed } from '@angular/core/testing';

import { LangSettingsService } from './lang-settings-service';

describe('LangSettingsService', () => {
  let service: LangSettingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LangSettingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
