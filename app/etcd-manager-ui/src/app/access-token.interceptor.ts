import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { AuthService } from './pages/service/auth.service';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';

const AUTH_ENDPOINT_FRAGMENT = '/api/auth/';

function withAuthHeaders(req: HttpRequest<unknown>, accessToken: string | null): HttpRequest<unknown> {
  if (!accessToken) {
    return req;
  }
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${accessToken}`,
      PortalAlias: location.origin
    }
  });
}

export const accessTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  return next(withAuthHeaders(req, authService.getAccessToken())).pipe(
    catchError((error) => {
      const isAuthEndpoint = req.url.toLowerCase().includes(AUTH_ENDPOINT_FRAGMENT);
      // F010: on 401 (except for the auth endpoints themselves), refresh the access
      // token once via the HttpOnly cookie and retry the original request.
      // Concurrent 401s share the single in-flight refresh inside AuthService.
      if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthEndpoint) {
        return authService.refreshAccessToken().pipe(
          switchMap((accessToken) => next(withAuthHeaders(req, accessToken))),
          catchError((refreshError) => {
            // refresh failed → session is gone: clear state and go to login
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
