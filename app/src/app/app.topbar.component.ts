import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MenuItem, PrimeIcons } from 'primeng/api';
import { AppMainComponent } from './app.main.component';
import { AppCtxService } from './service/app-ctx.service';
import { AuthenService } from './service/authen.service';
import { ComCtxService } from './service/com-ctx.service';
import { ConnectionService } from './service/connection.service';

@Component({
    selector: 'app-topbar',
    templateUrl: './app.topbar.component.html',
    styles: [`
    .user-topbar {
        height: 3rem;
        line-height: 3rem;
        margin-left:10px;
        display: inline-flex;
        justify-content: right;
        align-items: right;
    }
    .user-topbar:hover {
        cursor: pointer;

    }

    .user-topbar span {
        margin-left:10px;
    }
    `]
})
export class AppTopBarComponent implements OnInit {

    items: MenuItem[];
    connections: any[];
    selectedConnection: any;
    selfEvent = false;
    rootCtx: ComCtxService;
    user: any;
    userMenuItems: MenuItem[] = [
        {
            label: 'Change Password', icon: PrimeIcons.LOCK,
            command: this.changePassword.bind(this)
        },
        {
            label: 'Logout', icon: PrimeIcons.SIGN_OUT,
            command: this.logout.bind(this)
        },
    ];
    hasManageUserPermission = false;
    @Output() onClickPage = new EventEmitter<string>();

    constructor(public appMain: AppMainComponent,
        private _connectionService: ConnectionService,
        private _appCtxService: AppCtxService,
        public authenService: AuthenService) {
        this.rootCtx = this._appCtxService.getRootCtx();
        this._connectionService.getDataSource().then(rs => {
            this.connections = rs.data;
        });

        this.rootCtx.replaySubscribe('SELECT_CONNECTION', (connection: any) => {
            if (!this.selfEvent) {
                // select connection
                this.selectedConnection = connection.id;
            } else {
                this.selfEvent = false;
            }
        });

        this.rootCtx.replaySubscribe('RELOAD_DATASOURCE', () => {
            // reload datasource
            this.loadDataSource();
        });

        this.rootCtx.replaySubscribe('LOGGED_IN', async (user) => {
            this.user = user;
            this.hasManageUserPermission = await this.authenService.hasManagePermission();
        });
    }
    ngOnInit(): void {
        if (this.authenService.hasValidAccessToken()) {
            this.authenService.getUserInfo().then(async user => {
                this.user = user;
            });
            this.authenService.hasManagePermission().then(rs => {
                this.hasManageUserPermission = rs;
            });
        }
        this.loadDataSource();
        this.onClickPage.emit('/connection-manager')
    }

    changePassword() {

    }

    logout() {
        this.user = null;
        this.hasManageUserPermission = false;
        this.authenService.logout();
    }

    async loadDataSource() {
        this.connections = (await this._connectionService.getDataSource()).data.map((connection: any) => {
            connection.name = connection.name + ' - ' + connection.server + ' - ' + connection.username;
            return connection;
        });
    }

    onSelectConnection(evt) {
        console.log('select connection', evt);
        this.selfEvent = true;
        this.rootCtx.dispatchReplayEvent('SELECT_CONNECTION', this.connections.find(x => x.id == evt.value));
    }
}
