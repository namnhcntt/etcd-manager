import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { patchState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseService } from './base.service';
import { JwtPayload, jwtDecode } from "jwt-decode";

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {

  ACCESS_TOKEN_KEY = 'access_token';
  REFRESH_TOKEN_KEY = 'refresh_token';
  USERINFO_KEY = 'userinfo';
  ENDPOINT_AUTHEN_LOGIN = 'api/auth/login';

  private _router = inject(Router);

  hasValidAccessToken(): boolean {
    const token = this.getAccessTokenObject();
    if (token === null) {
      return false;
    }
    // check expire date of token
    const expireDate = new Date(token.exp! * 1000);
    if (new Date() > expireDate) {
      return false;
    }
    return true;
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getAccessTokenObject(): JwtPayload | null {
    const token = this.getAccessToken();
    if (token) {
      return jwtDecode(token);
    }
    return null;
  }

  login(userName: string, password: string): Promise<any> {
    const url = `${environment.apiEndpoint}/${this.ENDPOINT_AUTHEN_LOGIN}`;
    return firstValueFrom(this.httpClient.post<any>(url, { userName, password }));
  }

  localLogout() {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USERINFO_KEY);
    this._router.navigateByUrl('/login');
  }

  saveToken(accessToken: string, refreshToken: string) {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    this.loadUserStore();
  }

  loadUserStore() {
    const item = this.getAccessTokenObject();
    if (item) {
      const nameId: string = (item as any).nameid;
      const userInfo = { id: nameId, name: item.sub! };
      patchState(this.globalStore, { currentUser: userInfo });
    }
  }
}
