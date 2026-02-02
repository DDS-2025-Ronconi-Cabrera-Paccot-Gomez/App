using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TravelPro.Users.Dtos;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Users;


namespace TravelPro.Users;

[RemoteService]
[Route("api/profile")]
[Authorize]
public class ProfileAppService : TravelProAppService, IProfileAppService, ITravelProProfileAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public ProfileAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _currentUser = currentUser;
    }
    [HttpGet]
    public async Task<ProfileDto> GetAsync()
    {
        var user = await _userRepository.GetAsync(_currentUser.GetId());
        return ObjectMapper.Map<IdentityUser, ProfileDto>(user);
    }

    [HttpPut]
    public async Task<ProfileDto> UpdateAsync(UpdateProfileDto input)
    {
        if (!_currentUser.IsAuthenticated)
            throw new AbpAuthorizationException("Debe iniciar sesión.");

        var user = await _userRepository.GetAsync(_currentUser.GetId());

        // Actualizar datos estándar
        user.Name = input.Name;
        user.Surname = input.Surname;
        await _userManager.SetEmailAsync(user, input.Email);

        // Manejar ProfilePhoto
        if (input.ExtraProperties != null &&
            input.ExtraProperties.ContainsKey("ProfilePhoto"))
        {
            user.SetProperty("ProfilePhoto", input.ExtraProperties["ProfilePhoto"]);
        }

        (await _userManager.UpdateAsync(user)).CheckErrors();

        // Mapear nuevamente
        return ObjectMapper.Map<IdentityUser, ProfileDto>(user);
    }
    [HttpPost]
    [Route("change-password")]
    public async Task ChangePasswordAsync(ChangePasswordInput input)
    {
        if (!_currentUser.IsAuthenticated)
            throw new AbpAuthorizationException("Debe iniciar sesión.");

        var user = await _userRepository.GetAsync(_currentUser.GetId());

        (await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword))
            .CheckErrors();
    }
    [HttpDelete] // Importante: Verbo HTTP DELETE
    public async Task DeleteAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new AbpAuthorizationException("Debes iniciar sesión.");
        }

        var user = await _userManager.GetByIdAsync(_currentUser.GetId());

        // Esto borra al usuario (Soft Delete por defecto en ABP)
        (await _userManager.DeleteAsync(user)).CheckErrors();
    }

    [HttpGet]
    [Route("public/{userId}")] // Definimos una ruta clara: api/profile/public/{id}
    public async Task<PublicProfileDto> GetPublicProfileAsync(Guid userId)
    {
        // 1. Buscamos el usuario en la base de datos
        var user = await _userRepository.GetAsync(userId);

        // 2. Mapeamos MANUALMENTE para estar seguros de no filtrar datos sensibles
        // (O podés usar ObjectMapper si configuras un mapa específico, pero esto es más seguro ahora)

        var publicProfile = new PublicProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            // Recuperamos la foto de las propiedades extra
            ProfilePhoto = user.GetProperty<string>("ProfilePhoto")
        };

        return publicProfile;
    }

    [HttpGet]
    [Route("public/username/{userName}")] // Ruta nueva: api/profile/public/username/pepe
    public async Task<PublicProfileDto> GetPublicProfileByUserNameAsync(string userName)
    {
        // 1. Buscamos por Nombre de Usuario
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            throw new UserFriendlyException("El usuario no existe.");
        }

        // 2. Mapeamos al DTO Público (igual que antes)
        return new PublicProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            ProfilePhoto = user.GetProperty<string>("ProfilePhoto")
        };
    }

    [HttpGet]
    [Route("search")]
    public async Task<List<PublicProfileDto>> SearchAsync(string filter)
    {
        // Validación simple
        if (string.IsNullOrWhiteSpace(filter))
        {
            return new List<PublicProfileDto>();
        }

        // Usamos el repositorio. El parámetro 'filter' busca automáticamente
        // coincidencias en UserName, Email, Name y Surname.
        // maxResultCount: 10 para no traer mil usuarios de golpe.
        var users = await _userRepository.GetListAsync(filter: filter, maxResultCount: 10);

        // Mapeamos la lista de usuarios a nuestra lista de DTOs seguros
        var publicProfiles = new List<PublicProfileDto>();

        foreach (var user in users)
        {
            publicProfiles.Add(new PublicProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                ProfilePhoto = user.GetProperty<string>("ProfilePhoto")
            });
        }

        return publicProfiles;
    }
}
