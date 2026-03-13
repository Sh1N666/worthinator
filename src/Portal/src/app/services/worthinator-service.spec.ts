import { TestBed } from '@angular/core/testing';

import { WorthinatorService } from './worthinator-service';

describe('WorthinatorService', () => {
  let service: WorthinatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WorthinatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
