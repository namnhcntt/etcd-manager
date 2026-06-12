import { Component, OnInit, ViewEncapsulation, inject, model } from '@angular/core';
import { Router } from '@angular/router';
import { ToastMessageOptions } from 'primeng/api';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { LayoutService } from '../../layout/service/app.layout.service';
import { AuthService } from '../service/auth.service';
import { APP_BASE_HREF } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: [`
  :host ::ng-deep .p-password input {
    width: 100%;
    padding:1rem;
    }

    :host ::ng-deep .pi-eye{
      transform:scale(1.6);
      margin-right: 1rem;
      color: var(--primary-color) !important;
    }

    :host ::ng-deep .pi-eye-slash{
      transform:scale(1.6);
      margin-right: 1rem;
      color: var(--primary-color) !important;
    }

    .layout-main {
      width: 100%;
    }
  `],
  standalone: true,
  imports: [...commonLayoutImport, MessageModule, PasswordModule, InputTextModule],
  encapsulation: ViewEncapsulation.None
})
export class LoginComponent extends BaseComponent implements OnInit {

  userName = model('');
  password = model('');
  loginMessages: ToastMessageOptions[] = [];

  public layoutService = inject(LayoutService);
  router = inject(Router);
  authService = inject(AuthService);
  private readonly _baseHref: string = inject(APP_BASE_HREF);

  ngOnInit() {
    if (this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/');
    }
  }

  signIn() {
    this.loginMessages = [];
    if (!this.userName()) {
      this.loginMessages = [{ severity: 'error', detail: 'Please enter userName' }];
      return;
    }
    if (!this.password()) {
      this.loginMessages = [{ severity: 'error', detail: 'Please enter password' }];
      return;
    }

    this.authService.login(this.userName(), this.password()).then((res: any) => {
      // refresh token is delivered via HttpOnly cookie, never present in the body
      this.authService.saveToken(res.token);
      window.location.href = this._baseHref;
    }).catch(err => {
      this.loginMessages = [{ severity: 'error', summary: 'Error', detail: err.error.error }];
    });
  }
}
