import { Routes } from '@angular/router';

export const demoRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent)
  }
]
