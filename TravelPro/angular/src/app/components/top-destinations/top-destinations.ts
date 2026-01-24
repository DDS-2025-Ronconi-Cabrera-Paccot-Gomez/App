import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DestinationService } from '../../proxy/destinations/destination.service';
import { CityDto } from '../../proxy/destinations/dtos/models';
import { RatingService } from '../../proxy/ratings/rating.service'; // Para mostrar las estrellas

@Component({
  selector: 'app-top-destinations',
  standalone: true,
  imports: [CommonModule], // Importamos CommonModule para *ngFor
  templateUrl: './top-destinations.html',
  styleUrls: ['./top-destinations.scss']
})
export class TopDestinationsComponent implements OnInit {
  
  private destinationService = inject(DestinationService);
  private ratingService = inject(RatingService);

  topDestinations: any[] = []; // Usamos any para agregarle la propiedad 'averageScore' dinámicamente
  loading = true;

  ngOnInit() {
    this.loadTop();
  }

  loadTop() {
    this.destinationService.getTopDestinations().subscribe({
      next: (list) => {
        this.topDestinations = list;
        this.loading = false;
        
        // Opcional: Cargar el puntaje exacto para mostrarlo
        this.topDestinations.forEach(dest => {
           this.ratingService.getStatsByDestination(dest.id).subscribe(stats => {
              dest.averageScore = stats.averageScore;
              dest.totalCount = stats.totalCount;
           });
        });
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
}