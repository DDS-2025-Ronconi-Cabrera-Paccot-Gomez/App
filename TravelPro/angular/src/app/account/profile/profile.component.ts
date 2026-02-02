import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProfileService } from '../../proxy/users/profile.service';
import { ProfileDto, UpdateProfileDto } from '../../proxy/volo/abp/account/models';
import { UserAvatarService } from 'src/app/services/user-avatar.service';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { AuthService, RestService } from '@abp/ng.core';


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
    readonly defaultAvatar = 'https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png?20150327203541';
    password = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
    
};
    passwordErrors: string[] = [];
    emailError: string | null = null;
    nameError: string | null = null;
    surnameError: string | null = null;

    constructor(private profileService: ProfileService, private avatar: UserAvatarService,private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private authService: AuthService,  private restService: RestService ) {}

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

    get profilePic(): string {
    // Si hay foto en el perfil, la usa. Si no, usa el default.
    return (this.profile?.extraProperties?.['ProfilePhoto'] as string) || this.defaultAvatar;
  }

  // Manejo de error de imagen (si la URL base64 está rota)
  onImageError(event: any) {
    event.target.src = this.defaultAvatar;
  }
  toggleEdit() {
    this.editing = !this.editing;
    if (!this.editing) {
       this.load(); // Si cancela, recargamos los datos originales
    }
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
    console.log('Intentando cambiar contraseña...', this.password);

    this.validatePassword();
    
    if (this.passwordErrors.length > 0) {
      this.passwordErrors.forEach(e => this.toaster.error(e));
      return;
    }

    // Usamos RestService para llamar directamente al endpoint, evitando problemas de proxy
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/profile/change-password', // Ruta definida en tu ProfileAppService
      body: {
        currentPassword: this.password.currentPassword,
        newPassword: this.password.newPassword
      }
    }).subscribe({
      next: () => {
        this.toaster.success("Contraseña actualizada correctamente");
        // Limpiamos el formulario
        this.password = { currentPassword: '', newPassword: '', confirmPassword: '' };
        this.passwordErrors = [];
      },
      error: (err) => {
        console.error("Error cambiando contraseña", err);
        // Mostramos el mensaje exacto si el backend lo envía (ej: "Contraseña incorrecta")
        const msg = err.error?.error?.message || "Error al cambiar la contraseña. Verifica la actual.";
        this.toaster.error(msg);
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

deleteAccount() {
    this.confirmation
      .warn(
        '', // Asegurate de que esta key exista o pon un texto fijo
        '::AreYouSure',
        {
          messageLocalizationParams: [],
          titleLocalizationParams: [],
        }
      )
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.executeDelete();
        }
      });
  }

  private executeDelete() {
    // Llamamos al método delete() que creamos en el backend
    // Si TypeScript se queja de que 'delete' no existe, recuerda hacer 'abp generate-proxy -t ng'
    this.profileService.delete().subscribe({
      next: () => {
        this.toaster.success('Tu cuenta ha sido eliminada correctamente.', 'Adiós');
        
        // Cerramos la sesión inmediatamente para limpiar tokens inválidos
        this.authService.logout().subscribe();
      },
      error: (err) => {
        console.error('Error eliminando cuenta', err);
        this.toaster.error('Ocurrió un error al intentar eliminar la cuenta.', 'Error');
      }
    });
  }

}
