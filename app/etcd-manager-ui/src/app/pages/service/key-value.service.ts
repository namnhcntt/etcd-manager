import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class KeyValueService extends BaseService {

  readonly ENDPOINT_KEYVALUE = 'api/keyvalue';

  getAll(selectedConnectionId: number) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}`;
    return firstValueFrom(this.httpClient.get<any>(url, { params }));
  }

  getAllKeys(selectedConnectionId: number) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetAllKeys`;
    return firstValueFrom(this.httpClient.get<any[]>(url, { params }));
  }

  getByKey(selectedConnectionId: number, key: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString())
      .set('key', key);
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetByKey`;
    return firstValueFrom(this.httpClient.get<any>(url, { params }));
  }

  getByKeyPrefix(selectedConnectionId: number, keyPrefix: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString())
      .set('keyPrefix', keyPrefix);
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetByKeyPrefix`;
    return firstValueFrom(this.httpClient.get<any[]>(url, { params }));
  }

  isRootKey(key?: string | null): boolean {
    return key === '/';
  }

  save(selectedConnectionId: number, key: string, value: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/Save`;
    return firstValueFrom(this.httpClient.post<any>(url, { key, value }, { params }));
  }

  renameKey(selectedConnectionId: number, oldKey: string, newKey: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/RenameKey`;
    return firstValueFrom(this.httpClient.post<any>(url, { oldKey, newKey }, { params }));
  }

  deleteKey(selectedConnectionId: number, key: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString())
      .set('key', key);
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/DeleteKey`;
    return firstValueFrom(this.httpClient.delete<any>(url, { params }));
  }

  getRevision(selectedConnectionId: number, key: string) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString())
      .set('key', key);
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetRevision`;
    return firstValueFrom(this.httpClient.get<any>(url, { params }));
  }

  getRevisionDetail(selectedConnectionId: number, key: string, revision: number) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString())
      .set('key', key)
      .set('revision', revision.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/GetRevisionDetail`;
    return firstValueFrom(this.httpClient.get<any>(url, { params }));
  }

  importNodes(selectedConnectionId: number, nodes: any[]) {
    const params = new HttpParams()
      .set('selectedEtcdConnectionId', selectedConnectionId.toString());
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_KEYVALUE}/ImportNodes`;
    return firstValueFrom(this.httpClient.post<any>(url, nodes, { params }));
  }
}
