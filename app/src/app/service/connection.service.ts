import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Guid } from 'guid-ts';
import { firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ConnectionService {

    readonly STORAGE_KEY = 'etcdmanager_data';
    readonly ENDPOINT_CHECK_CONNECTION = 'connection/checkconnection';
    constructor(
        private _httpClient: HttpClient
    ) {

    }

    checkConnection(connection: any): Promise<string> {
        const arr = connection.server.split(':');
        const host = arr[0];
        const port = arr.length > 1 ? arr[1] : 2379;
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_CHECK_CONNECTION}?server=${host}&port=${port}&username=${connection.username}&password=${connection.password}&insecure=${connection.insecure}`;
        return firstValueFrom(this._httpClient.get<string>(url));
    }

    update(connection: any): string {
        const ds = this.getDataSource();
        const existItem = ds.find(item => item.id === connection.id);
        if (existItem) {
            const rs = this.validate(ds, connection);
            if (rs) { return rs; }
            existItem.name = connection.name;
            existItem.server = connection.server;
            existItem.username = connection.username;
            existItem.password = connection.password;
            existItem.insecure = connection.insecure;
            localStorage.setItem(this.STORAGE_KEY, JSON.stringify(ds));
        } else {
            return 'item does not exist';
        }
        return '';
    }

    insert(connection: any): string {
        const ds = this.getDataSource();
        const rs = this.validate(ds, connection);
        if (rs) { return rs; }
        connection.createdAt = new Date();
        connection.id = Guid.newGuid().toString();
        ds.push(connection);
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(ds));
        return '';
    }

    validate(ds: any[], connection: any): string {
        if (ds.some(x => x.name == connection.name)) {
            return 'connection name already exists';
        }
        const existItem = ds.find(x => x.server == connection.server && x.username == connection.username && x.password == connection.password);
        if (existItem) {
            return 'connection with server, username and password already exists with name "' + existItem.name + "\"";
        }
        return '';
    }

    deleteByName(name: string) {
        const ds = this.getDataSource();
        const index = ds.findIndex(x => x.name == name);
        if (index >= 0) {
            ds.splice(index, 1);
            localStorage.setItem(this.STORAGE_KEY, JSON.stringify(ds));
        }
    }

    getByName(name: string): any {
        const ds = this.getDataSource();
        return ds.find(x => x.name == name);
    }

    getDataSource() {
        const data = localStorage.getItem(this.STORAGE_KEY);
        if (data) {
            return JSON.parse(data);
        }
        return [];
    }
}
