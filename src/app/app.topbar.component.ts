import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { AppMainComponent } from './app.main.component';
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
        private _connectionService: ConnectionService) {
        this.connections = this._connectionService.getDataSource();
    }
    ngOnInit(): void {
        this.onClickPage.emit('/connection-manager');
    }
}
