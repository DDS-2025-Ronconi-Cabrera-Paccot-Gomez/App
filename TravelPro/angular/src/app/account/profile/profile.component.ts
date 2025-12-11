import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProfileService } from '../../proxy/users/profile.service';
import { ProfileDto, UpdateProfileDto } from '../../proxy/volo/abp/account/models';
import { UserAvatarService } from 'src/app/services/user-avatar.service';

@Component({
    standalone: true,
    selector: 'app-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.scss'],
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
})
export class ProfileComponent implements OnInit {
    
    profile: ProfileDto | null = null;
    editing = false;
    loading = false;
    password = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
    
};
    passwordErrors: string[] = [];
    emailError: string | null = null;
    nameError: string | null = null;
    surnameError: string | null = null;

    constructor(private profileService: ProfileService, private avatar: UserAvatarService) {}

    ngOnInit() {
        this.load();
    }

    load() {
        this.loading = true;

        this.profileService.get().subscribe({
        next: (x) => {
            // aseguramos que extraProperties exista
            x.extraProperties = x.extraProperties ?? {};
            this.profile = x;
        },
        error: (err) => console.error('Error cargando perfil', err),
        complete: () => this.loading = false
        });
    }
//VALIDACIONES
    validatePassword() {
    const errors: string[] = [];
    const p = this.password.newPassword;

    if (!p || p.length < 8) errors.push("Debe tener al menos 8 caracteres.");
    if (!/[A-Z]/.test(p)) errors.push("Debe contener al menos una letra mayúscula.");
    if (!/[a-z]/.test(p)) errors.push("Debe contener al menos una letra minúscula.");
    if (!/[0-9]/.test(p)) errors.push("Debe contener al menos un número.");
    if (!/[^A-Za-z0-9]/.test(p)) errors.push("Debe contener al menos un carácter especial.");

    if (this.password.confirmPassword !== p)
        errors.push("Las contraseñas no coinciden.");

    this.passwordErrors = errors;
}

validateEmail() {
    const email = this.profile?.email ?? '';
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    

    if (!regex.test(email)) {
        this.emailError = "El email no es válido.";
    } else {
        this.emailError = null;
    }
}

validateName() {
    const value = this.profile?.name;

    
    if (!value) {
        this.nameError = null;
        return;
    }

    const regex = /^[A-Za-zÁÉÍÓÚÑáéíóúñ ]{2,}$/;

    if (!regex.test(value)) {
        this.nameError = "El nombre debe tener al menos 2 letras y solo contener caracteres alfabéticos.";
    } else {
        this.nameError = null;
    }
}

validateSurname() {
    const value = this.profile?.surname;

    
    if (!value) {
        this.surnameError = null;
        return;
    }

    const regex = /^[A-Za-zÁÉÍÓÚÑáéíóúñ ]{2,}$/;

    if (!regex.test(value)) {
        this.surnameError = "El apellido debe tener al menos 2 letras y solo contener caracteres alfabéticos.";
    } else {
        this.surnameError = null;
    }
}

//GUARDAR

    save() {

        this.validateEmail();
        this.validateName();
        this.validateSurname();

        if (this.emailError || this.nameError || this.surnameError) {
            alert("Hay errores en el formulario. Corrígelos antes de guardar.");
            return;
        }

        if (!this.profile) return;

        const input: UpdateProfileDto = {
        userName: this.profile.userName,
        email: this.profile.email,
        name: this.profile.name,
        surname: this.profile.surname,
        phoneNumber: this.profile.phoneNumber,
        concurrencyStamp: this.profile.concurrencyStamp!,
        extraProperties: {
            ProfilePhoto: this.profile.extraProperties?.['ProfilePhoto'] ?? null
        }
        };

        this.profileService.update(input).subscribe({
        next: () => {
            this.editing = false;
            this.load();
            this.avatar.update(input.extraProperties?.ProfilePhoto ?? null);
        },
        error: (err) => console.error('Error guardando perfil', err)
        });
    }

        changePassword() {
    this.validatePassword();
    if (this.passwordErrors.length > 0) {
        return;
    }

    if (this.password.newPassword !== this.password.confirmPassword) {
        alert("Las contraseñas no coinciden");
        return;
    }

    this.profileService.changePassword({
        currentPassword: this.password.currentPassword,
        newPassword: this.password.newPassword
    }).subscribe({
        next: () => {
        alert("Contraseña actualizada correctamente");

        // limpiar campos
        this.password = { currentPassword: '', newPassword: '', confirmPassword: '' };
        },
        error: (err) => {
        console.error("Error cambiando contraseña", err);
        alert("Error cambiando la contraseña");
        }
    });
}

onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
        const base64 = reader.result as string;

        // asignar imagen al perfil
        if (this.profile) {
        this.profile.extraProperties = this.profile.extraProperties ?? {};
        this.profile.extraProperties["ProfilePhoto"] = base64;
        }
    };

    reader.readAsDataURL(file);
}


}
