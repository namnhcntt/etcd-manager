import { inject } from '@angular/core';
import { globalStore } from '../../global.store';
import { HttpClient } from '@angular/common/http';

export abstract class BaseService {
  protected readonly httpClient = inject(HttpClient);
  protected readonly globalStore = inject(globalStore);
}
