/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { AuthenService } from './authen.service';

describe('Service: Authen', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthenService]
    });
  });

  it('should ...', inject([AuthenService], (service: AuthenService) => {
    expect(service).toBeTruthy();
  }));
});
