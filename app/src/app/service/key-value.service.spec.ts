/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { KeyValueService } from './key-value.service';

describe('Service: KeyValue', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [KeyValueService]
    });
  });

  it('should ...', inject([KeyValueService], (service: KeyValueService) => {
    expect(service).toBeTruthy();
  }));
});
