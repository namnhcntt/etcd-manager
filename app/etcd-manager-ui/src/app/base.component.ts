import { inject } from '@angular/core';
import { globalStore } from './global.store';

export abstract class BaseComponent {
  public globalStore = inject(globalStore);
}
