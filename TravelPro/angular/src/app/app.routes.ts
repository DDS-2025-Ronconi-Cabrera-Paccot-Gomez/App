import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
import { Destinations } from './destinations/destinations';
import { replaceAccountManageRoute } from './account/manage/replace.manage.routing';

export const APP_ROUTES: Routes = [
  replaceAccountManageRoute,
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () =>
      import('@abp/ng.account').then(c => c.createRoutes()),
  },

  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  { path: 'destinos', component: Destinations },
{
  path: 'account/profile',
  loadComponent: () =>
    import('./account/profile/profile.component').then(m => m.ProfileComponent),
},

];
