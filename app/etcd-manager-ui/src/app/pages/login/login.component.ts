import { Component, OnInit, ViewEncapsulation, inject, model } from '@angular/core';
import { Router } from '@angular/router';
import { Message, MessageService } from 'primeng/api';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
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
  imports: [...commonLayoutImport, MessagesModule, PasswordModule, InputTextModule],
  providers: [],
  encapsulation: ViewEncapsulation.None
})
export class LoginComponent extends BaseComponent implements OnInit {

  userName = model('');
  password = model('');

  msgs1: Message[] = [];

  public layoutService = inject(LayoutService);
  router = inject(Router);
  authService = inject(AuthService);
  private readonly _baseHref: string = inject(APP_BASE_HREF);
  private readonly _messageService = inject(MessageService);

  ngOnInit() {
    if (this.authService.hasValidAccessToken()) {
      this.router.navigateByUrl('/');
    }
  }

  signIn() {
    this.msgs1.length = 0;
    if (!this.userName()) {
      this.msgs1 = [{ severity: 'error', summary: 'Error', detail: 'Please enter userName' }];
      return;
    }
    if (!this.password()) {
      this.msgs1 = [{ severity: 'error', summary: 'Error', detail: 'Please enter password' }];
      return;
    }

    this.authService.login(this.userName(), this.password()).then((res: any) => {
      this.authService.saveToken(res.token, res.refreshToken);
      window.location.href = this._baseHref;
    }).catch(err => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error, key: 'login' });
    });
  }
}
