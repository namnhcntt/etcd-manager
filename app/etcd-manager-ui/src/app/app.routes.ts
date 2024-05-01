import { Routes } from '@angular/router';
import { AppLayoutComponent } from './layout/app.layout.component';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';

export const routes: Routes = [
  {
    path: '', component: AppLayoutComponent,
    children: [

    ]
  },
  { path: 'auth', loadChildren: () => import('./demo/components/auth/auth.routes').then(m => m.authRoutes) },
  { path: 'notfound', component: NotfoundComponent },
  { path: '**', redirectTo: '/notfound' },
];
