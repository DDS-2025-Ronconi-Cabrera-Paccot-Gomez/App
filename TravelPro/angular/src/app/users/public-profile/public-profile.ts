import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router'; // Para leer la URL
import { ProfileService } from '../../proxy/users/profile.service'; // Tu servicio
import { PublicProfileDto } from '../../proxy/users/dtos/models'; // El DTO nuevo
import { Location } from '@angular/common';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-profile.html',
  styleUrls: ['./public-profile.scss']
})
export class PublicProfileComponent implements OnInit {
  
  private route = inject(ActivatedRoute);
  private profileService = inject(ProfileService);
  private location = inject(Location);

  userProfile: PublicProfileDto | null = null;
  loading = true;
  error = false;

  ngOnInit() {
    // Nos suscribimos a los cambios en la URL
    this.route.params.subscribe(params => {
      const userNameParam = params['id'];
      if (userNameParam) {
        this.loadProfile(userNameParam);
      }
    });
  }

  private loadProfile(userName: string) {
    this.loading = true;
    this.error = false;

    this.profileService.getPublicProfileByUserName(userName).subscribe({
      next: (result) => {
        this.userProfile = result;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = true;
        this.loading = false;
      }
    });
  }
  goBack(): void {
    this.location.back();
  }
}