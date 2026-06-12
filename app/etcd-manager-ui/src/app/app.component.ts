import { Component, OnInit, effect, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { MessageService } from 'primeng/api';
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
  authService = inject(AuthService);
  router = inject(Router);
  private readonly _etcdConnectionService = inject(EtcdConnectionService);
  private readonly _messageService = inject(MessageService);

  displaySidebar = false;

  constructor() {
    super();

    effect(() => {
      if (this.globalStore.displaySidebar.connectionManager()
        || this.globalStore.displaySidebar.userManager()
        || this.globalStore.displaySidebar.etcdUserManager()
        || this.globalStore.displaySidebar.etcdRoleManager()
        || this.globalStore.displaySidebar.etcdSnapshotManager()) {
        this.displaySidebar = true;
      } else {
        this.displaySidebar = false;
      }
    });

    effect(() => {
      if (this.globalStore.currentUser().id) {
        this.loadDataSourceConnections();
      }
    });
  }

  async ngOnInit(): Promise<void> {
    // the access token lives in memory only, so a page reload loses it:
    // attempt a silent refresh via the HttpOnly refresh-token cookie — but only when
    // the session hint says a cookie may exist (avoids a doomed call on every
    // anonymous page load, which would eat into the auth rate limit)
    if (!this.authService.hasValidAccessToken() && this.authService.hasSessionHint()) {
      try {
        await firstValueFrom(this.authService.refreshAccessToken());
      } catch {
        // no valid session — fall through to the login redirect below
      }
    }
    this.globalStore.setReadyRenderPage(true);
    if (!this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/login');
    }
    // when refresh succeeded, saveToken → loadUserStore → currentUser effect
    // already triggers loadDataSourceConnections
  }

  private loadDataSourceConnections() {
    this._etcdConnectionService.getDataSource().then((data: any) => {
      this.globalStore.setDataSourceConnections(data.connections);
    }).catch((err) => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    });
  }
}
