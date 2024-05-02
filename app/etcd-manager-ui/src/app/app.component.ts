import { Component, OnInit, ViewChild, effect, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { patchState } from '@ngrx/signals';
import { PrimeNGConfig } from 'primeng/api';
import { Sidebar, SidebarModule } from 'primeng/sidebar';
import { BaseComponent } from './base.component';
import { ConnectionManagerComponent } from './pages/connection-manager/connection-manager.component';
import { AuthService } from './pages/service/auth.service';
import { UserManagerComponent } from './pages/user-manager/user-manager.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidebarModule, ConnectionManagerComponent, UserManagerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent extends BaseComponent implements OnInit {
  title = 'etcd-manager-ui';
  primengConfig = inject(PrimeNGConfig);
  authService = inject(AuthService);
  router = inject(Router);

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
      }
    });
  }

  ngOnInit(): void {
    this.primengConfig.ripple = true;
    patchState(this.globalStore, { readyRenderPage: true });
    if (!this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/login');
    } else {
      this.authService.loadUserStore();
    }
  }

  closeForm(formCode: string) {

  }
}
