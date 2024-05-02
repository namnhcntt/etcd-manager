import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class EtcdConnectionService extends BaseService {

  readonly ENDPOINT_CONNECTION = 'api/etcdconnection';

  constructor() {
    super();
  }

  getDataSource() {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}`;
    return firstValueFrom(this.httpClient.get<any[]>(url));
  }

  delete(id: string) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}/${id}`;
    return firstValueFrom(this.httpClient.delete(url));
  }

  update(id: string, item: any) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}/${id}`;
    return firstValueFrom(this.httpClient.put(url, item));
  }

  insert(item: any) {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}`;
    return firstValueFrom(this.httpClient.post(url, item));
  }
}
