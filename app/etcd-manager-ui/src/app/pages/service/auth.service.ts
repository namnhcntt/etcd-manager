import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { JwtPayload, jwtDecode } from "jwt-decode";
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseService } from './base.service';
import { APP_BASE_HREF } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {

  ACCESS_TOKEN_KEY = 'access_token';
  REFRESH_TOKEN_KEY = 'refresh_token';
  USERINFO_KEY = 'userinfo';
  ENDPOINT_AUTHEN_LOGIN = 'api/auth/login';

  private readonly _router = inject(Router);
  private readonly _baseHref: string = inject(APP_BASE_HREF);

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

  loggedIn(): boolean {
    return this.getAccessToken() !== null;
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

  logout() {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USERINFO_KEY);
    window.location.href = `${this._baseHref}/login`;
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
      this.globalStore.setCurrentUser(userInfo);
    }
  }
}
