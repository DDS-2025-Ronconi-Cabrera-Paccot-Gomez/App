import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
import { Destinations } from './destinations/destinations';
import { replaceAccountManageRoute } from './account/manage/replace.manage.routing';
import { PublicProfileComponent } from './users/public-profile/public-profile';
import { UserSearchComponent } from './users/user-search/user-search';
import { TopDestinationsComponent } from './components/top-destinations/top-destinations'; 

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
{
    path: 'users/:id', // ':id' indica que es un valor variable
    component: PublicProfileComponent,
    canActivate: [authGuard] // Solo usuarios logueados pueden ver perfiles
  },

  {
  path: 'search',
  component: UserSearchComponent,
  canActivate: [authGuard]
},
{
    path: 'popular-destinations',
    component: TopDestinationsComponent,
    // canActivate: [AuthGuard] // Descomenta si quieres que solo lo vean usuarios logueados
  },

];
