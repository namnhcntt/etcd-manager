import { inject } from '@angular/core';
import { globalStore } from '../../global.store';
import { HttpClient } from '@angular/common/http';

export abstract class BaseService {
  httpClient = inject(HttpClient);
  globalStore = inject(globalStore);
}
