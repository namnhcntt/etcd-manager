import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { AuthenService } from '../service/authen.service';

@Injectable({
    providedIn: 'root'
})
export class SendAccessTokenInterceptor implements HttpInterceptor {

    constructor(private _authenService: AuthenService,
    ) { }

    private handleAuthError(err: HttpErrorResponse): Observable<any> {
        //handle your auth error or rethrow
        if (err.status === 401) {
            //navigate /delete cookies or whatever
            this._authenService.localLogout();
            // if you've caught / handled the error, you don't want to rethrow it unless you also want downstream consumers to have to handle it as well.
            return of(err.message); // or EMPTY may be appropriate here
        }
        return throwError(err);
    }


    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const accessToken = this._authenService.getAccessToken();
        if (accessToken) {
            request = request.clone({
                setHeaders: {
                    Authorization: `${accessToken}`,
                }
            });
        }

        return next.handle(request).pipe(catchError(x => this.handleAuthError(x)));
    }
}
