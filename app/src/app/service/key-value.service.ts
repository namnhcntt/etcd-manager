import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResponseModel } from '../models/response.model';

@Injectable({
  providedIn: 'root'
})
export class KeyValueService {

  readonly ENDPOINT_GET_KEY_BY_CONNECTION = 'keyvalue/getall';
  readonly ENDPOINT_GET_DETAIL_BY_KEY = 'keyvalue/get';
  constructor(
    private _httpClient: HttpClient
  ) { }

  getAll(connection: any): Promise<ResponseModel> {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_GET_KEY_BY_CONNECTION}`;
    return firstValueFrom(this._httpClient.post<any>(url, connection));
  }

  getByKey(connection: any, key: string): Promise<ResponseModel> {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_GET_DETAIL_BY_KEY}?key=${encodeURIComponent(key)}`;
    return firstValueFrom(this._httpClient.post<ResponseModel>(url, connection));
  }
}
