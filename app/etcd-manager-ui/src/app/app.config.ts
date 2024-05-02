import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { accessTokenInterceptor } from './access-token.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      accessTokenInterceptor
    ])),
    provideAnimationsAsync(),
    importProvidersFrom(CommonModule),
    importProvidersFrom(ToastModule),
    importProvidersFrom(ConfirmPopupModule),
    importProvidersFrom(ConfirmDialogModule),
    MessageService, ConfirmationService,
  ]
};
