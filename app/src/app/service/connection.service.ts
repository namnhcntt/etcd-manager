import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Guid } from 'guid-ts';
import { firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResponseModel } from '../models/response.model';

@Injectable({
    providedIn: 'root'
})
export class ConnectionService {

    readonly STORAGE_KEY = 'etcdmanager_data';
    readonly ENDPOINT_CHECK_CONNECTION = 'connection/checkconnection';
    readonly ENDPOINT_DELETE_BY_NAME = 'connection/deleteconnectionbyname';
    readonly ENDPOINT_CONNECTION = 'connection';

    constructor(
        private _httpClient: HttpClient
    ) {

    }

    checkConnection(connection: any): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CHECK_CONNECTION}`;
        if (!connection.id) {
            connection = { ...connection, id: Guid.newGuid().toString() };
        }
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, connection));
    }

    update(connection: any): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}/${connection.id}`;
        return firstValueFrom(this._httpClient.put<ResponseModel>(url, connection));
    }

    insert(connection: any): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}`;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, connection));
    }

    validate(ds: any[], connection: any): string {
        if (ds.some(x => x.name == connection.name && x.id != connection.id && x.enableAuthenticated == connection.enableAuthenticated)) {
            return 'connection name already exists';
        }
        if (connection.enableAuthenticated) {
            const existItem = ds.find(x =>
                x.server == connection.server
                && x.username == connection.username
                && x.password == connection.password
                && x.enableAuthenticated == connection.enableAuthenticated
                && x.id != connection.id);
            if (existItem) {
                return 'connection with server, username and password already exists with name "' + existItem.name + "\"";
            }
        } else {
            const existItem = ds.find(x => x.server == connection.server && x.enableAuthenticated == false);
            if (existItem) {
                return 'connection with server, username and password already exists with name "' + existItem.name + "\"";
            }
        }
        return '';
    }

    deleteByName(name: string) {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_DELETE_BY_NAME}?name=${name}`;
        return firstValueFrom(this._httpClient.delete<ResponseModel>(url));
    }

    getByName(name: string): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}/GetByName?name=${name}`;
        return firstValueFrom(this._httpClient.get<ResponseModel>(url));
    }

    getDataSource() {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CONNECTION}`;
        return firstValueFrom(this._httpClient.get<ResponseModel>(url));
    }
}
