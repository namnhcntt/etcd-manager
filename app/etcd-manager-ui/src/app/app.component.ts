import { Component, OnInit, effect, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { patchState } from '@ngrx/signals';
import { MessageService, PrimeNGConfig } from 'primeng/api';
import { SidebarModule } from 'primeng/sidebar';
import { BaseComponent } from './base.component';
import { ConnectionManagerComponent } from './pages/connection-manager/connection-manager.component';
import { AuthService } from './pages/service/auth.service';
import { UserManagerComponent } from './pages/user-manager/user-manager.component';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { EtcdConnectionService } from './pages/service/etcd-connection.service';
import { LocalCacheService } from './pages/service/local-cache.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidebarModule, ConnectionManagerComponent, UserManagerComponent,
    ConfirmPopupModule, ConfirmDialogModule, ToastModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent extends BaseComponent implements OnInit {
  title = 'etcd-manager-ui';
  primengConfig = inject(PrimeNGConfig);
  authService = inject(AuthService);
  router = inject(Router);
  private _etcdConnectionService = inject(EtcdConnectionService);
  private _messageService = inject(MessageService);
  private _localCacheService = inject(LocalCacheService);

  dipslaySidebar = false;

  constructor() {
    super();

    effect(() => {
      if (this.globalStore.dipslaySidebar.connectionManager()
        || this.globalStore.dipslaySidebar.userManager()
        || this.globalStore.dipslaySidebar.etcdUserManager()
        || this.globalStore.dipslaySidebar.etcdRoleManager()
        || this.globalStore.dipslaySidebar.etcdSnapshotManager()) {
        this.dipslaySidebar = true;
      } else {
        this.dipslaySidebar = false;
      }
    });

    this._etcdConnectionService.getDataSource().then((data: any) => {
      patchState(this.globalStore, { connections: { ...this.globalStore.connections(), dataSource: data.connections } });
    }).catch((err) => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    });
  }

  ngOnInit(): void {
    this.primengConfig.ripple = true;
    patchState(this.globalStore, { readyRenderPage: true });
    // patchState(this.globalStore, { dipslaySidebar: { ...this.globalStore.dipslaySidebar(), connectionManager: true } });
    if (!this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/login');
    } else {
      this.authService.loadUserStore();
    }
  }

  closeForm(formCode: string) {

  }
}
