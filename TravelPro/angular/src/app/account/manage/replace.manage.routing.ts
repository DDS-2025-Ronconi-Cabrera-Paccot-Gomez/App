import { Route } from '@angular/router';

export const replaceAccountManageRoute: Route = {
    path: 'account/manage',
    loadComponent: () =>
    import('../profile/profile.component').then(m => m.ProfileComponent),
};
