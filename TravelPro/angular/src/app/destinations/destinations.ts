import { Component, OnInit, inject, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
// 1. Importamos CoreModule directamente (esto reemplaza al SharedModule)
import { ListResultDto, CoreModule, ConfigStateService } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';

// Rutas relativas al proxy
import { DestinationService } from '../proxy/destinations/destination.service';
import { CityDto, SearchDestinationsInputDto } from '../proxy/destinations/dtos/models';
import { RatingService, RatingDto, CreateUpdateRatingDto, RatingStatsDto } from '../proxy/ratings';


@Component({
  selector: 'app-search-destinations',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    CoreModule,
    NgbModule
  ],
  templateUrl: './destinations.html',
  styleUrls: ['./destinations.scss'],
})
export class Destinations implements OnInit {
  
  private readonly destinationService = inject(DestinationService);
  private readonly ratingService = inject(RatingService);
  private readonly modalService = inject(NgbModal);
  private readonly configState = inject(ConfigStateService);

  destinations: CityDto[] = []; 
  loading = false;

  searchParams: SearchDestinationsInputDto = {
    partialName: '',
  };

  readonly defaultImage = 'assets/images/destination-placeholder.svg';
  // --- DATOS DE RATINGS ---
  cityStats: { [cityId: string]: RatingStatsDto } = {};
  myReviews: { [cityId: string]: RatingDto } = {};

  // Variables para el Modal de Calificar
  selectedCity: CityDto | null = null;
  currentRatingForm: CreateUpdateRatingDto = { score: 5, comment: '', destinationId: '', userId:''  };
  currentReviewId: string | null = null; // Si estamos editando, guardamos el ID aquí

  // Variables para el Modal de Listado
  cityReviewsList: RatingDto[] = [];
  loadingReviews = false;

  // Helpers visuales
  stars = [1, 2, 3, 4, 5];

  get currentUserId(): string {
    return this.configState.getOne('currentUser')?.id;
  }

  ngOnInit(): void {
  }

  private loadCities(): void {
    if (!this.searchParams.partialName || this.searchParams.partialName.length < 3) {
      this.destinations = [];
      return;
    }

    this.loading = true;
    this.destinations = [];
    this.cityStats = {}; // Limpiamos stats anteriores

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
          this.loadExtrasForCities();
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

  // --- PUNTO 5.1, 5.2 y 5.3: ABRIR MODAL DE CALIFICAR ---
  openRateModal(content: TemplateRef<any>, city: CityDto): void {
  const cityId = (city as any).id; 
  if (!this.currentUserId) {
    alert('Debes iniciar sesión para calificar.');
    return;
  }

  this.selectedCity = city;
  const existingReview = this.myReviews[cityId];

  if (existingReview) {
    // MODO EDICIÓN
    this.currentReviewId = existingReview.id;
    this.currentRatingForm = {
      destinationId: cityId,
      score: existingReview.score,
      comment: existingReview.comment,
      userId: this.currentUserId // <--- ASIGNAR ID REAL AQUÍ
    };
  } else {
    // MODO CREACIÓN
    this.currentReviewId = null;
    this.currentRatingForm = {
      destinationId: cityId,
      score: 5,
      comment: '',
      userId: this.currentUserId // <--- ASIGNAR ID REAL AQUÍ
    };
  }

  this.modalService.open(content, { centered: true });
}

saveRating(modal: any): void {
    if (!this.selectedCity || !this.currentUserId) return;

    // 1. OBTENER EL ID (Ahora es seguro porque CityDto tiene Id)
    // Si viene 0000... significa que es nueva.
    const cityId = (this.selectedCity as any).id || (this.selectedCity as any).Id;

    if (!cityId) {
      alert('Error: No se pudo identificar el ID de la ciudad.');
      return;
    }

    // 2. PREPARAR DATOS (Sanitizados)
    const destinationData: any = {
      name: this.selectedCity.name || 'Ciudad sin nombre',
      country: this.selectedCity.country || 'Desconocido',
      population: this.selectedCity.population || 0,
      region: (this.selectedCity as any).region || (this.selectedCity as any).adminCode1 || 'Sin Región', 
      coordinates: {
        latitude: (this.selectedCity.coordinates?.latitude || 0).toString(),
        longitude: (this.selectedCity.coordinates?.longitude || 0).toString()
      },
      lastUpdated: new Date().toISOString(), 
      photo: (this.selectedCity as any).photo || "" 
    };

    this.loading = true;

    // 3. SINCRONIZAR
    this.destinationService.sync(cityId, destinationData)
      .subscribe({
        // IMPORTANTE: Capturamos el resultado (syncedCity) que devuelve el backend
        next: (syncedCity) => { 
          
          // ¡AQUÍ ESTÁ EL CAMBIO!
          // Usamos el ID que nos devolvió el backend (syncedCity.id), no el cityId viejo.
          // Si la ciudad era nueva, syncedCity.id tendrá el nuevo GUID real.
          this.currentRatingForm.destinationId = syncedCity.id; 
          
          // Ahora sí guardamos la reseña
          this.executeSaveRatingLogic(modal);
        },
        error: (err) => {
          console.error('Error al sincronizar el destino:', err);
          const errorMsg = err.error?.error?.message || 'Error desconocido';
          alert(`Error al preparar el destino: ${errorMsg}`);
          this.loading = false;
        }
      });
  }

  // Extraje la lógica de guardar/editar reseña a una función aparte para no repetir código
  private executeSaveRatingLogic(modal: any): void {
    const request = this.currentReviewId
      ? this.ratingService.update(this.currentReviewId, this.currentRatingForm)
      : this.ratingService.create(this.currentRatingForm);

    request
      .pipe(finalize(() => this.loading = false)) // Apagar spinner al final
      .subscribe({
        next: () => {
          this.finishRatingAction(modal);
        },
        error: (err) => {
          console.error('Error al guardar reseña:', err);
          // Aquí puedes mostrar un toaster de error
        }
      });
  }

  deleteRating(modal: any): void {
    if (!this.currentReviewId) return;
    if(confirm('¿Seguro que quieres eliminar tu reseña?')) {
      this.ratingService.delete(this.currentReviewId).subscribe(() => {
        this.finishRatingAction(modal);
      });
    }
  }

  private finishRatingAction(modal: any): void {
    modal.close();
    const selectedCityId = (this.selectedCity as any).id; 
    // Recargamos los datos de esa ciudad específica para actualizar la vista
    if (this.selectedCity) {
      this.refreshCityData(selectedCityId);
    }
  }

  // --- PUNTO 5.5: ABRIR MODAL DE VER RESEÑAS ---
  openReviewsModal(content: TemplateRef<any>, city: CityDto): void {
    const cityId = (city as any).id; 
    this.selectedCity = city;
    this.loadingReviews = true;
    this.cityReviewsList = [];

    this.modalService.open(content, { size: 'lg', centered: true, scrollable: true });

    this.ratingService.getListByDestination(cityId)
      .pipe(finalize(() => this.loadingReviews = false))
      .subscribe(list => {
        this.cityReviewsList = list;
      });
  }

  // --- UTILIDADES ---

  // Refresca stats y reviews de una sola ciudad tras editar
  private refreshCityData(cityId: string): void {
      this.ratingService.getStatsByDestination(cityId).subscribe(s => this.cityStats[cityId] = s);
      
      if (this.currentUserId) {
        this.ratingService.getListByDestination(cityId).subscribe(list => {
           const myReview = list.find(r => r.creatorId === this.currentUserId);
           if (myReview) this.myReviews[cityId] = myReview;
           else delete this.myReviews[cityId];
        });
      }
  }

  setScore(score: number): void {
    this.currentRatingForm.score = score;
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
  // --- AGREGAR ESTA FUNCIÓN DENTRO DE LA CLASE ---

 private loadExtrasForCities(): void {
    this.destinations.forEach(city => {
      
      // 1. TRUCO DE SEGURIDAD: Intentamos leer id (minúscula) o Id (mayúscula)
      const cityId = (city as any).id || (city as any).Id;

      // 2. DEBUG: Si quieres ver en la consola del navegador qué llega (F12)
      // console.log('Procesando ciudad:', city.name, 'ID:', cityId);

      // 3. VALIDACIÓN: Si el ID es undefined o null, SALTAMOS esta vuelta.
      // Esto evita que se llame al backend con basura y salga el error rojo.
      if (!cityId) {
        console.warn('Se encontró una ciudad sin ID, saltando stats...', city);
        return;
      }

      // --- A PARTIR DE AQUÍ ES SEGURO LLAMAR ---

      // 1. Cargar Estadísticas
      this.ratingService.getStatsByDestination(cityId).subscribe(stats => {
        this.cityStats[cityId] = stats;
      });

      // 2. Buscar si yo ya califiqué
      if (this.currentUserId) {
        this.ratingService.getListByDestination(cityId).subscribe(list => {
           const myReview = list.find(r => r.creatorId === this.currentUserId);
           
           if (myReview) {
             this.myReviews[cityId] = myReview;
           } else {
             delete this.myReviews[cityId]; 
           }
        });
      }
    });
  }
}