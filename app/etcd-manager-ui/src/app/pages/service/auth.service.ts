import { Injectable, inject } from '@angular/core';
import { JwtPayload, jwtDecode } from "jwt-decode";
import { Observable, firstValueFrom } from 'rxjs';
import { finalize, map, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { BaseService } from './base.service';
import { APP_BASE_HREF } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {

  private static readonly ENDPOINT_AUTHEN_LOGIN = 'api/auth/login';
  private static readonly ENDPOINT_AUTHEN_REFRESH = 'api/auth/token/refresh';
  private static readonly ENDPOINT_AUTHEN_LOGOUT = 'api/auth/logout';

  // F009: access token lives in memory only (never localStorage/sessionStorage) so an
  // XSS payload cannot exfiltrate it from storage. The refresh token never reaches JS
  // at all — it is kept in an HttpOnly cookie set by the API.
  private accessToken: string | null = null;
  // single in-flight refresh shared by all concurrent callers (prevents refresh stampede)
  private refreshInFlight$: Observable<string> | null = null;

  private readonly _baseHref: string = inject(APP_BASE_HREF);

  constructor() {
    super();
    // purge tokens persisted by previous app versions
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('userinfo');
  }

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
    return this.accessToken;
  }

  getAccessTokenObject(): JwtPayload | null {
    const token = this.getAccessToken();
    if (token) {
      return jwtDecode(token);
    }
    return null;
  }

  login(userName: string, password: string): Promise<any> {
    const url = `${environment.apiEndpoint}/${AuthService.ENDPOINT_AUTHEN_LOGIN}`;
    // withCredentials so the browser stores the HttpOnly refresh-token cookie
    return firstValueFrom(this.httpClient.post<any>(url, { userName, password }, { withCredentials: true }));
  }

  /**
   * Exchanges the HttpOnly refresh-token cookie for a new access token.
   * Concurrent callers share the same in-flight request.
   */
  refreshAccessToken(): Observable<string> {
    if (!this.refreshInFlight$) {
      const url = `${environment.apiEndpoint}/${AuthService.ENDPOINT_AUTHEN_REFRESH}`;
      this.refreshInFlight$ = this.httpClient.post<any>(url, {}, { withCredentials: true }).pipe(
        map(res => {
          this.saveToken(res.token);
          return res.token as string;
        }),
        finalize(() => {
          this.refreshInFlight$ = null;
        }),
        shareReplay(1)
      );
    }
    return this.refreshInFlight$;
  }

  async logout(): Promise<void> {
    try {
      // revokes the stored refresh token server-side and clears the HttpOnly cookie
      const url = `${environment.apiEndpoint}/${AuthService.ENDPOINT_AUTHEN_LOGOUT}`;
      await firstValueFrom(this.httpClient.post(url, {}, { withCredentials: true }));
    } catch {
      // best effort — still clear local state
    }
    this.accessToken = null;
    window.location.href = `${this._baseHref}/login`;
  }

  saveToken(accessToken: string) {
    this.accessToken = accessToken;
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
