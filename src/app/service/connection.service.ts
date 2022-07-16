import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class ConnectionService {

    readonly STORAGE_KEY = 'etcdmanager_data';

    constructor() { }

    getDataSource() {
        const data = localStorage.getItem(this.STORAGE_KEY);
        if (data) {
            return JSON.parse(data);
        }
        return [];
    }
}
