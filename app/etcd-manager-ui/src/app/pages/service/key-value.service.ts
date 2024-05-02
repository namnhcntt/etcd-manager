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
}
