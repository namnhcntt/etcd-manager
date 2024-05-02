import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalCacheService {
  // using localStorage as a cache store
  private cache = window.localStorage;

  constructor() { }

  set(key: string, value: any): void {
    this.cache.setItem(key, JSON.stringify(value));
  }

  get(key: string): any {
    const value = this.cache.getItem(key);
    return value ? JSON.parse(value) : null;
  }

  getObject<T>(key: string, defaultValue: T): T {
    const value = this.get(key);
    return value ? value : defaultValue;
  }
}
