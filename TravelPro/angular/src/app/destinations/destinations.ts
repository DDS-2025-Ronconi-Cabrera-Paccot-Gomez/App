import { Component, OnInit, inject, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
// Importamos RestService para llamadas manuales seguras
import { ListResultDto, CoreModule, ConfigStateService, RestService } from '@abp/ng.core';
import { Subject, of } from 'rxjs';
import { switchMap, delay, tap, catchError, finalize, debounceTime } from 'rxjs/operators';

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
  private searchTrigger$ = new Subject<void>();

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
    this.searchTrigger$.pipe(
    debounceTime(1200),
    switchMap(() => {
      
    const hasCountry = !!this.searchParams.country;
    const hasName = this.searchParams.partialName && this.searchParams.partialName.length >= 3;

    if (!hasCountry && !hasName) {
      console.warn("⚠️ Abortando: Se requiere al menos un país o un nombre de 3 letras.");
      this.loading = false;
      return of({ items: [] });
    }
      this.loading = true;
      // Sincronizamos nombres antes de la petición
      const countryObj = this.countries.find(c => c.code === this.searchParams.country);
      const regionObj = this.regions.find(r => r.code === this.searchParams.region);
      const requestPayload = {
      ...this.searchParams,
      _version: new Date().getTime(),
      // Si está vacío, enviamos null. Esto suele ser la clave.
      region: this.searchParams.region || null, 
      minPopulation: this.searchParams.minPopulation || null,
      country: this.searchParams.country || null,
      countryName: countryObj ? countryObj.name : '',
      regionName: regionObj ? regionObj.name : ''
      
    };

    return this.destinationService.searchCities(requestPayload).pipe(
        // CatchError aquí para que si falla una vez, el gatillo siga vivo para la próxima
        catchError(error => {
            console.error("❌ Error en la API:", error);
            this.loading = false;
            return of({ items: [] });
        })
      );

    })
  ).subscribe({
    next: (res) => {
      this.destinations = res.items || [];
      this.loading = false;
      if (this.destinations.length > 0) this.loadExtrasForCities();
    },
    error: () => this.loading = false
  });

  // 2. SEGUNDO: Cargamos los países para que el selector se llene al iniciar
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


onCountryChange(): void {
  // 1. Limpiamos selección previa
  this.searchParams.region = ''; 
  this.searchParams.regionName = ''; 
  this.regions = [];

  if (!this.searchParams.country) {
    this.searchTrigger$.next();
    //this.onSearch();
    return;
  }

  this.destinationService.getRegions(this.searchParams.country)
    .subscribe({
      next: (list) => {
        this.regions = list || [];
        setTimeout(() => {
        this.searchTrigger$.next();
      }, 100);
      },
      error: (err) => {
        console.error("❌ Error cargando regiones", err);
        this.searchTrigger$.next()
      }
    });
}




onSearch(): void {
    this.searchTrigger$.next()
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

  const targetIndex = this.destinations.findIndex(d => {
    // 1. Prioridad absoluta: ¿Es exactamente el mismo objeto en memoria?
    // Esto funciona el 99% de las veces en Angular cuando vienes de un *ngFor
    if (d === this.selectedCity) return true;

    // 2. Si no es la misma referencia (ej: copias), miramos los IDs
    const dId = (d as any).id || (d as any).Id;
    const sId = (this.selectedCity as any).id || (this.selectedCity as any).Id;

    // Helper para detectar IDs "basura" o vacíos
    const isValidId = (id: any) => {
        if (!id) return false;
        if (id === '00000000-0000-0000-0000-000000000000') return false; // Empty GUID
        return true;
    };

    // Solo comparamos por ID si AMBOS tienen un ID válido (guardado en DB)
    if (isValidId(dId) && isValidId(sId)) {
        return dId == sId;
    }

    // Si llegamos aquí, los objetos son distintos y no tienen ID válido.
    // No son el mismo.
    return false;
});

  console.log(`🎯 Índice capturado para actualizar: ${targetIndex}`, this.destinations[targetIndex]);


  if (targetIndex === -1) {
      alert("Error crítico: No encuentro el destino en la lista local.");
      return;
  }
    // El ID que usamos para buscar en GeoDB
    const originalId = (this.selectedCity as any).id || (this.selectedCity as any).Id;

  

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

    this.destinationService.sync(originalId, destinationData)
      .subscribe({
        next: (syncedCity) => { 

        if (this.destinations[targetIndex]) {
            console.log("🔄 Reemplazando destino en posición:", targetIndex);
          // Reemplazamos el objeto viejo por el sincronizado (que ya tiene el ID de DB)
          this.destinations[targetIndex] = { ...syncedCity };
          // Actualizamos también la ciudad seleccionada para que el resto del proceso use el ID nuevo
          this.selectedCity = this.destinations[targetIndex];

          this.destinations = [...this.destinations];
        }
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

    if (this.selectedCity) {
      const dbId = (this.selectedCity as any).id;
      this.refreshCityData(dbId);

      this.destinations = [...this.destinations];
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