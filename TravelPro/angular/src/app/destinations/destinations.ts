import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
// 1. Importamos CoreModule directamente (esto reemplaza al SharedModule)
import { ListResultDto, CoreModule } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';

// Rutas relativas al proxy
import { DestinationService } from '../proxy/destinations/destination.service';
import { CityDto, SearchDestinationsInputDto } from '../proxy/destinations/dtos/models';

// (Eliminé el import de Coordinate y SharedModule que daban error)

@Component({
  selector: 'app-search-destinations',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    // 2. Usamos CoreModule aquí para tener acceso al pipe 'abpLocalization'
    CoreModule 
  ],
  templateUrl: './destinations.html',
  styleUrls: ['./destinations.scss'],
})
export class Destinations implements OnInit {
  
  private readonly destinationService = inject(DestinationService);

  destinations: CityDto[] = []; 
  loading = false;

  searchParams: SearchDestinationsInputDto = {
    partialName: '',
  };

  readonly defaultImage = 'assets/images/destination-placeholder.svg';

  ngOnInit(): void {
  }

  private loadCities(): void {
    if (!this.searchParams.partialName || this.searchParams.partialName.length < 3) {
      this.destinations = [];
      return;
    }

    this.loading = true;

    this.destinationService
      .searchCities(this.searchParams)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (result: ListResultDto<CityDto>) => {
          this.destinations = result.items || [];
        },
        error: (error) => {
          console.error('Error al cargar ciudades:', error);
          this.destinations = [];
        },
      });
  }

  onSearch(): void {
    this.loadCities();
  }

  clearSearch(): void {
    this.searchParams.partialName = '';
    this.destinations = [];
  }

  onImageError(event: any): void {
    event.target.src = this.defaultImage;
  }

  // --- FUNCIONES AUXILIARES ---

  formatPopulation(population?: number): string {
    if (population === undefined || population === null) {
      return 'N/A';
    }
    return population.toLocaleString('es-ES');
  }

  formatCoordinates(coordinates: any): string {
    if (!coordinates || !coordinates.latitude || !coordinates.longitude) {
      return 'N/A';
    }
    const lat = parseFloat(coordinates.latitude);
    const lng = parseFloat(coordinates.longitude);
    return `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
  }

 getDestinationImage(city: any): string {
    return this.defaultImage;
  }

  openInMaps(destination: any): void {
    if (destination && destination.coordinates) {
        const lat = destination.coordinates.latitude;
        const lng = destination.coordinates.longitude;
        const url = `https://www.google.com/maps?q=${lat},${lng}`;
        window.open(url, '_blank');
    }
  }
}