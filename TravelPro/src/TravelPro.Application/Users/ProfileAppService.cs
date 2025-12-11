using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
public class ProfileAppService : TravelProAppService, IProfileAppService
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
}
