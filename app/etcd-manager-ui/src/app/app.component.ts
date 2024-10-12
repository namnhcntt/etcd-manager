import { ChangeDetectionStrategy, Component, OnInit, effect, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MessageService, PrimeNGConfig } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { SidebarModule } from 'primeng/sidebar';
import { ToastModule } from 'primeng/toast';
import { BaseComponent } from './base.component';
import { ConnectionManagerComponent } from './pages/connection-manager/connection-manager.component';
import { AuthService } from './pages/service/auth.service';
import { EtcdConnectionService } from './pages/service/etcd-connection.service';
import { UserManagerComponent } from './pages/user-manager/user-manager.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidebarModule, ConnectionManagerComponent, UserManagerComponent,
    ConfirmPopupModule, ConfirmDialogModule, ToastModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',

})
export class AppComponent extends BaseComponent implements OnInit {
  title = 'etcd-manager-ui';
  primengConfig = inject(PrimeNGConfig);
  authService = inject(AuthService);
  router = inject(Router);
  private readonly _etcdConnectionService = inject(EtcdConnectionService);
  private readonly _messageService = inject(MessageService);

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

    effect(() => {
      if (this.globalStore.currentUser().id) {
        this.loadDataSource();
      }
    });
  }

  ngOnInit(): void {
    if (this.authService.loggedIn()) {
      this.loadDataSource();
    }
    this.primengConfig.ripple = true;
    this.globalStore.setReadyRenderPage(true);
    if (!this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/login');
    } else {
      this.authService.loadUserStore();
    }
  }

  private loadDataSource() {
    this._etcdConnectionService.getDataSource().then((data: any) => {
      this.globalStore.setDataSource(data.connections);
    }).catch((err) => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    });
  }
}
