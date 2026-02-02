import { Component, inject } from '@angular/core';
import { AuthService, LocalizationPipe } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [CommonModule],
  standalone: true
})
export class HomeComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated
  }

  login() {
    this.authService.navigateToLogin();
  }
    // --- Funciones de Navegación ---
  
  navigateToDestinations() {
    // Asegúrate de que en app.routes.ts tengas una ruta path: 'destinations'
    this.router.navigate(['/destinos']); 
  }

  navigateToPopular() {
    // Ruta para el componente de Destinos Populares
    this.router.navigate(['/popular-destinations']);
  }

  navigateToUsers() {
    // Ruta para el buscador de usuarios (la que definimos anteriormente como 'search')
    this.router.navigate(['/search']); 
  }
}
