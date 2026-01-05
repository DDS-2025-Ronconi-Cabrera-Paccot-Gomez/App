import { Injectable } from '@angular/core';
import { ProfileService } from '../proxy/users/profile.service';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserAvatarService {
    private avatarUrl$ = new BehaviorSubject<string | null>(null);

    avatarChanged$ = this.avatarUrl$.asObservable();

    constructor(private profileService: ProfileService) {}

    /** Cargar avatar desde backend */
    load() {
        return this.profileService.get().subscribe(p => {
        const url = p.extraProperties?.ProfilePhoto ?? null;
        this.avatarUrl$.next(url);
        });
    }

    /** Actualizar avatar manualmente (cuando el usuario lo cambia) */
    update(url: string | null) {
        this.avatarUrl$.next(url);
    }

    /** Obtener valor actual */
    get url() {
        return this.avatarUrl$.value;
    }
}
