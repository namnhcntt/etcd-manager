import { Component, OnInit, OnDestroy } from '@angular/core';
import { ConfigService } from '../../service/app.config.service';
import { AppConfig } from '../../api/appconfig';
import { Subscription } from 'rxjs';
import { Message } from 'primeng/api';
import { AuthenService } from 'src/app/service/authen.service';
import { Router } from '@angular/router';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
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
  `]
})
export class LoginComponent implements OnInit, OnDestroy {
    password: string;
    userName: string;
    config: AppConfig;
    subscription: Subscription;
    msgs1: Message[] = [];
    rootCtx: ComCtxService;

    constructor(
        public configService: ConfigService,
        private _authenService: AuthenService,
        private _router: Router,
        private _appCtxService: AppCtxService
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
    }

    ngOnInit(): void {
        this.config = this.configService.config;
        this.subscription = this.configService.configUpdate$.subscribe(config => {
            this.config = config;
        });
    }

    ngOnDestroy(): void {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    signIn() {
        this.msgs1.length = 0;
        if (!this.userName) {
            this.msgs1 = [{ severity: 'error', summary: 'Error', detail: 'Please enter username' }];
            return;
        }
        if (!this.password) {
            this.msgs1 = [{ severity: 'error', summary: 'Error', detail: 'Please enter password' }];
            return;
        }
        this._authenService.login(this.userName, this.password).then(async rs => {
            if (rs.success) {
                this._authenService.saveToken(rs.data);
                // get userinfo
                const userInfo = await this._authenService.getUserInfo();
                this.rootCtx.dispatchReplayEvent('LOGGED_IN', userInfo);
                this._router.navigate(['/']);
            } else {
                this.msgs1 = [{ severity: 'error', summary: 'Error', detail: rs.message }];
            }
        });
    }
}
