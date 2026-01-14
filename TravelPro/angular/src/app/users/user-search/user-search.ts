import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Necesario para el input
import { ProfileService } from '../../proxy/users/profile.service'; // Tu servicio
import { PublicProfileDto } from '../../proxy/users/dtos/models'; // El DTO nuevo

@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-search.html',
  styleUrls: ['./user-search.scss']
})
export class UserSearchComponent {
  
  private profileService = inject(ProfileService);
  private router = inject(Router);

  searchTerm = '';
  searchResults: PublicProfileDto[] = [];
  hasSearched = false; // Para saber si ya buscó y mostrar "No se encontraron resultados"
  loading = false;

  search() {
    if (!this.searchTerm.trim()) return;

    this.loading = true;
    this.hasSearched = false; // Reseteamos estado visual

    this.profileService.search(this.searchTerm).subscribe({
      next: (results) => {
        this.searchResults = results;
        this.loading = false;
        this.hasSearched = true;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        this.searchResults = []; // Limpiamos en caso de error
      }
    });
  }

  goToProfile(userName: string) {
    this.router.navigate(['/users', userName]);
  }
}