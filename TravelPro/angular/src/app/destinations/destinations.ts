import { Component, OnInit, inject, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
// Importamos RestService para llamadas manuales seguras
import { ListResultDto, CoreModule, ConfigStateService, RestService } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';

// Proxies
import { DestinationService } from '../proxy/destinations/destination.service';
import { CityDto, SearchDestinationsInputDto, CountryDto } from '../proxy/destinations/dtos/models';
import { RatingService, RatingDto, CreateUpdateRatingDto, RatingStatsDto } from '../proxy/ratings';

// Definición manual de RegionDto por seguridad
export interface RegionDto {
  name?: string;
  code?: string;
}

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
  private readonly restService = inject(RestService); // Inyectamos RestService

  destinations: CityDto[] = []; 
  countries: CountryDto[] = [];
  regions: RegionDto[] = []; 
  loading = false;

  searchParams: SearchDestinationsInputDto = {
    partialName: '',
    minPopulation: null,
    country: '', 
    region: '' 
  };

  readonly defaultImage = 'assets/images/destination-placeholder.svg';
  cityStats: { [cityId: string]: RatingStatsDto } = {};
  myReviews: { [cityId: string]: RatingDto } = {};

  selectedCity: CityDto | null = null;
  currentRatingForm: CreateUpdateRatingDto = { score: 5, comment: '', destinationId: '', userId:''  };
  currentReviewId: string | null = null; 

  cityReviewsList: RatingDto[] = [];
  loadingReviews = false;

  stars = [1, 2, 3, 4, 5];

  get currentUserId(): string {
    return this.configState.getOne('currentUser')?.id;
  }

  ngOnInit(): void {
    this.loadCountries();
  }

  // CORREGIDO: Usamos RestService para asegurar la carga de países
  private loadCountries(): void {
    this.restService.request<any, CountryDto[]>({
      method: 'GET',
      url: '/api/app/destination/countries', // Ruta explícita del DestinationAppService
    }).subscribe({
      next: (list) => {
        this.countries = list;
      },
      error: (err) => console.error('Error cargando países:', err)
    });
  }

  // CORREGIDO: Usamos RestService para cargar regiones y desbloquear el menú
  onCountryChange(): void {
    this.searchParams.region = ''; 
    this.regions = [];

    if (!this.searchParams.country) {
      this.onSearch();
      return;
    }

    this.loading = true;
    
    // Llamada manual al endpoint de regiones en DestinationAppService
    this.restService.request<any, RegionDto[]>({
      method: 'GET',
      url: '/api/app/destination/regions', 
      params: { countryCode: this.searchParams.country }
    })
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (list) => {
          this.regions = list;
          this.onSearch();
        },
        error: (err) => {
          console.error('Error cargando regiones:', err);
          // Si falla, intentamos buscar igual sin regiones para no bloquear al usuario
          this.onSearch(); 
        }
      });
  }

  private loadCities(): void {
    const hasName = this.searchParams.partialName && this.searchParams.partialName.length >= 3;
    const hasCountry = this.searchParams.country && this.searchParams.country.length >= 2;
    const hasPop = this.searchParams.minPopulation && this.searchParams.minPopulation > 0;
    
    if (!hasName && !hasCountry && !hasPop) {
      this.destinations = [];
      return;
    }

    this.loading = true;
    this.destinations = [];
    this.cityStats = {}; 

    this.destinationService
      .searchCities(this.searchParams)
      .pipe(finalize(() => this.loading = false))
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
    this.searchParams.country = '';
    this.searchParams.minPopulation = null;
    this.searchParams.region = '';
    this.regions = []; 
    this.destinations = [];
  }
  
  onImageError(event: any): void {
    event.target.src = this.defaultImage;
  }

  openRateModal(content: TemplateRef<any>, city: CityDto): void {
    const cityId = (city as any).id; 
    if (!this.currentUserId) {
      alert('Debes iniciar sesión para calificar.');
      return;
    }
  
    this.selectedCity = city;
    const existingReview = this.myReviews[cityId];
  
    if (existingReview) {
      this.currentReviewId = existingReview.id;
      this.currentRatingForm = {
        destinationId: cityId,
        score: existingReview.score,
        comment: existingReview.comment,
        userId: this.currentUserId
      };
    } else {
      this.currentReviewId = null;
      this.currentRatingForm = {
        destinationId: cityId,
        score: 5,
        comment: '',
        userId: this.currentUserId
      };
    }
  
    this.modalService.open(content, { centered: true });
  }

  saveRating(modal: any): void {
    if (!this.selectedCity || !this.currentUserId) return;

    const cityId = (this.selectedCity as any).id || (this.selectedCity as any).Id;

    if (!cityId) {
      alert('Error: No se pudo identificar el ID de la ciudad.');
      return;
    }

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
      photo: "" 
    };

    this.loading = true;

    this.destinationService.sync(cityId, destinationData)
      .subscribe({
        next: (syncedCity) => { 
          this.currentRatingForm.destinationId = syncedCity.id; 
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

  private executeSaveRatingLogic(modal: any): void {
    const request = this.currentReviewId
      ? this.ratingService.update(this.currentReviewId, this.currentRatingForm)
      : this.ratingService.create(this.currentRatingForm);

    request
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.finishRatingAction(modal);
        },
        error: (err) => console.error('Error saving rating', err)
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
    if (this.selectedCity) {
      this.refreshCityData(selectedCityId);
    }
  }

  openReviewsModal(content: TemplateRef<any>, city: CityDto): void {
    const cityId = (city as any).id; 
    this.selectedCity = city;
    this.loadingReviews = true;
    this.cityReviewsList = [];

    this.modalService.open(content, { size: 'lg', centered: true, scrollable: true });

    if (!cityId || cityId === '00000000-0000-0000-0000-000000000000') {
       this.loadingReviews = false;
       return;
    }

    this.ratingService.getListByDestination(cityId)
      .pipe(finalize(() => this.loadingReviews = false))
      .subscribe(list => {
        this.cityReviewsList = list;
      });
  }

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

  formatPopulation(population?: number): string {
    if (population === undefined || population === null) return 'N/A';
    return population.toLocaleString('es-ES');
  }

  formatCoordinates(coordinates: any): string {
    if (!coordinates || !coordinates.latitude || !coordinates.longitude) return 'N/A';
    return `${parseFloat(coordinates.latitude).toFixed(4)}, ${parseFloat(coordinates.longitude).toFixed(4)}`;
  }

  getDestinationImage(city: any): string {
    return this.defaultImage;
  }

  openInMaps(destination: any): void {
    if (destination?.coordinates) {
        const url = `https://www.google.com/maps?q=${destination.coordinates.latitude},${destination.coordinates.longitude}`;
        window.open(url, '_blank');
    }
  }

  private loadExtrasForCities(): void {
    this.destinations.forEach(city => {
      const cityId = (city as any).id || (city as any).Id;
      if (!cityId || cityId === '00000000-0000-0000-0000-000000000000') return;

      this.ratingService.getStatsByDestination(cityId).subscribe(stats => {
        this.cityStats[cityId] = stats;
      });

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