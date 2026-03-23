import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/map', pathMatch: 'full' },

  // Auth routes
  { path: 'auth/login',    loadComponent: () => import('./auth/components/login.component').then(m => m.LoginComponent) },
  { path: 'auth/register', loadComponent: () => import('./auth/components/register.component').then(m => m.RegisterComponent) },

  // Shell (public + protected children)
  {
    path: '',
    loadComponent: () => import('./shared/components/shell.component').then(m => m.ShellComponent),
    children: [
      // Public — map visible to everyone
      { path: 'map', loadComponent: () => import('./map/components/map.component').then(m => m.MapComponent) },

      // Protected
      { path: 'journeys', canActivate: [authGuard], loadComponent: () => import('./journey/components/journey-list.component').then(m => m.JourneyListComponent) },
      { path: 'planner',  canActivate: [authGuard], loadComponent: () => import('./shared/components/placeholder.component').then(m => m.PlaceholderComponent), data: { title: 'Planner' } },
      { path: 'profile',  canActivate: [authGuard], loadComponent: () => import('./shared/components/placeholder.component').then(m => m.PlaceholderComponent), data: { title: 'Profile' } },
    ]
  },

  { path: '**', redirectTo: '/map' }
];
