import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { AppMainComponent } from './app.main.component';
import { AppEventService } from './service/app-event.service';
import { ConnectionService } from './service/connection.service';

@Component({
    selector: 'app-topbar',
    templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent implements OnInit {

    items: MenuItem[];
    connections: any[];
    selectedConnection: any;
    selfEvent = false;

    @Output() onClickPage = new EventEmitter<string>();

    constructor(public appMain: AppMainComponent,
        private _connectionService: ConnectionService,
        private _appEventService: AppEventService) {
        this.connections = this._connectionService.getDataSource();
        this._appEventService.getSubscriptionConnection().subscribe((connection: any) => {
            if (!this.selfEvent) {
                // select connection
                this.selectedConnection = connection.name;
            } else {
                this.selfEvent = false;
            }
        });
        this._appEventService.getSubscriptionReloadDataSource(() => {
            // reload datasource
            this.loadDataSource();
        });
    }
    ngOnInit(): void {
        this.loadDataSource();
    }

    loadDataSource() {
        this.connections = this._connectionService.getDataSource().map((connection: any) => {
            connection.name = connection.name + ' - ' + connection.server + ' - ' + connection.username;
            return connection;
        });
    }

    onSelectConnection(evt) {
        console.log('select connection', evt);
        this.selfEvent = true;
        this._appEventService.selectConnection(this.connections.find(x => x.id == evt.value));
    }
}
