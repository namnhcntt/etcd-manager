import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AppEventService {

    private _selectConnectionEvent = new Subject();
    private _reloadDataSourceEvent = new Subject();

    selectConnection(connection: any) {
        this._selectConnectionEvent.next(connection);
    }

    reloadDataSource() {
        this._reloadDataSourceEvent.next(true);
    }

    getSubscriptionConnection() {
        return this._selectConnectionEvent;
    }

    getSubscriptionReloadDataSource(callback: () => void) {
        this._reloadDataSourceEvent.subscribe(callback);
    }

    unsubscribeConnection() {
        this._selectConnectionEvent.unsubscribe();
    }

    unsubscribeReloadDataSource() {
        this._reloadDataSourceEvent.unsubscribe();
    }
}
