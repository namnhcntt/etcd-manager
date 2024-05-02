import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { PrimeNGConfig } from 'primeng/api';
import { AuthService } from './pages/service/auth.service';
import { BaseComponent } from './base.component';
import { patchState } from '@ngrx/signals';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent extends BaseComponent implements OnInit {
  title = 'etcd-manager-ui';
  primengConfig = inject(PrimeNGConfig);
  authService = inject(AuthService);
  router = inject(Router);

  ngOnInit(): void {
    this.primengConfig.ripple = true;
    patchState(this.globalStore, { readyRenderPage: true });
    if (!this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/login');
    } else {
      this.authService.loadUserStore();
    }
  }
}
