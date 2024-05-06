import { Routes } from '@angular/router';
import { AppLayoutComponent } from './layout/app.layout.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';

export const routes: Routes = [
  {
    path: '', component: AppLayoutComponent,
    children: [
      {
        path: '',
        loadChildren: () => import('./pages/pages.routes').then(m => m.pageRoutes)
      }
    ]
  },
  { path: 'notfound', component: NotFoundComponent },
  { path: '**', redirectTo: '/notfound' },
];
