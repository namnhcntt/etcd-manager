import { HttpInterceptorFn } from '@angular/common/http';
import { AuthService } from './pages/service/auth.service';
import { inject } from '@angular/core';

export const accessTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AuthService);
  const accessToken = accountService.getAccessToken();
  if (accessToken) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
        PortalAlias: location.origin
      }
    });
  }
  return next(req);
};
