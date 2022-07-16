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

    @Output() onClickPage = new EventEmitter<string>();

    constructor(public appMain: AppMainComponent,
        private _connectionService: ConnectionService,
        private _appEventService: AppEventService) {
        this.connections = this._connectionService.getDataSource();
        this._appEventService.getSubscriptionConnection().subscribe((connection: any) => {
            // select connection
            this.selectedConnection = connection.name;
        });
        this._appEventService.getSubscriptionReloadDataSource(() => {
            // reload datasource
            this.loadDataSource();
        });
    }
    ngOnInit(): void {
        this.onClickPage.emit('/connection-manager');
        this.loadDataSource();
    }

    loadDataSource() {
        this.connections = this._connectionService.getDataSource();
    }
}
