import { Directive, ElementRef, OnInit } from '@angular/core';
import { UserAvatarService } from '../services/user-avatar.service';

@Directive({
    selector: '[appUserAvatar]',
    standalone: true
    })
    export class UserAvatarDirective implements OnInit {

    constructor(
        private el: ElementRef,
        private avatar: UserAvatarService
    ) {}

    ngOnInit() {
        // 1) aplicar al cargar
        setTimeout(() => this.applyAvatar(), 300);

        // 2) re-aplicar cuando cambie
        this.avatar.avatarChanged$.subscribe(() => {
        setTimeout(() => this.applyAvatar(), 100);
        });
    }

    private applyAvatar() {
        if (!this.avatar.url) return;

        const host = this.el.nativeElement as HTMLElement;
        const avatars = host.querySelectorAll('.lpx-avatar');
        if (!avatars.length) return;

        avatars.forEach(avatarElem => {
        avatarElem.querySelectorAll('i, svg, lpx-icon').forEach(x => {
            (x as HTMLElement).style.display = 'none';
        });

        (avatarElem as HTMLElement).setAttribute(
            'style',
            `
            width: 35px !important;
            height: 35px !important;
            border-radius: 50%;
            background-image: url('${this.avatar.url}');
            background-size: cover;
            background-position: center;
            background-repeat: no-repeat;
            display: inline-block;
            `
        );
        });
    }
}
