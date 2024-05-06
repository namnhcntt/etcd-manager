import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class KeyValueService extends BaseService {

  readonly ENDPOINT_KEYVALUE = 'api/keyvalue';

  getAll(selectedConnectionId: number) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}?selectedEtcdConnectionId=${selectedConnectionId}`;
    return firstValueFrom(this.httpClient.get<any>(url));
  }

  getAllKeys(selectedConnectionId: number) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetAllKeys?selectedEtcdConnectionId=${selectedConnectionId}`;
    return firstValueFrom(this.httpClient.get<any[]>(url));
  }

  getByKey(selectedConnectionId: number, key: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetByKey?selectedEtcdConnectionId=${selectedConnectionId}&key=${key}`;
    return firstValueFrom(this.httpClient.get<any>(url));
  }

  getByKeyPrefix(selectedConnectionId: number, keyPrefix: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetByKeyPrefix?selectedEtcdConnectionId=${selectedConnectionId}&keyPrefix=${keyPrefix}`;
    return firstValueFrom(this.httpClient.get<any[]>(url));
  }

  isRootKey(key?: string | null): boolean {
    return key === '/';
  }

  save(selectedConnectionId: number, key: string, value: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/Save?selectedEtcdConnectionId=${selectedConnectionId}`;
    return firstValueFrom(this.httpClient.post<any>(url, { key, value }));
  }

  renameKey(selectedConnectionId: number, oldKey: string, newKey: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/RenameKey?selectedEtcdConnectionId=${selectedConnectionId}`;
    return firstValueFrom(this.httpClient.post<any>(url, { oldKey, newKey }));
  }

  deleteKey(selectedConnectionId: number, key: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/DeleteKey?selectedEtcdConnectionId=${selectedConnectionId}&key=${key}`;
    return firstValueFrom(this.httpClient.delete<any>(url));
  }

  getRevision(selectedConnectionId: number, key: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetRevision?selectedEtcdConnectionId=${selectedConnectionId}&key=${key}`;
    return firstValueFrom(this.httpClient.get<any>(url));
  }

  getRevisionDetail(selectedConnectionId: number, key: string, revision: number) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetRevisionDetail?selectedEtcdConnectionId=${selectedConnectionId}&key=${key}&revision=${revision}`;
    return firstValueFrom(this.httpClient.get<any>(url));
  }

  importNodes(selectedConnectionId: number, nodes: any[]) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/ImportNodes?selectedEtcdConnectionId=${selectedConnectionId}`;
    return firstValueFrom(this.httpClient.post<any>(url, nodes));
  }
}
