import { RoutesService,eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
import { configureUserMenu } from '@abp/ng.theme.lepton-x';


export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
    configureUserMenu();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
      path: '/destinos', // La URL (debe coincidir con la de app.routes.ts)
      name: 'Destinos', // El texto que se verá en el menú
      iconClass: 'fas fa-map-marked-alt', // Un ícono de mapa 
      order: 2, // Para que aparezca después de 'Hogar'
      layout: eLayoutType.application,
    }
  ]);


}
