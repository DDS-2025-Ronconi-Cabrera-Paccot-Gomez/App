using System;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;


namespace TravelPro;

public static class TravelProDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            // en TravelProDtoExtensions.Configure()
            
            ObjectExtensionManager.Instance.AddOrUpdateProperty(
                typeof(IdentityUserDto),
                typeof(string),
                "ProfilePhoto"
            );

            ObjectExtensionManager.Instance.AddOrUpdateProperty(
                typeof(IdentityUserCreateDto),
                typeof(string),
                "ProfilePhoto"
            );

            ObjectExtensionManager.Instance.AddOrUpdateProperty(
                typeof(IdentityUserUpdateDto),
                typeof(string),
                "ProfilePhoto"
            );

            // DTOs propios
            ObjectExtensionManager.Instance.AddOrUpdateProperty(typeof(UpdateProfileDto), typeof(string), "ProfilePhoto");
            ObjectExtensionManager.Instance.AddOrUpdateProperty(typeof(ProfileDto), typeof(string), "ProfilePhoto");

            /* You can add extension properties to DTOs
             * defined in the depended modules.
             *
             * Example:
             *
             * ObjectExtensionManager.Instance
             *   .AddOrUpdateProperty<IdentityRoleDto, string>("Title");
             *
             * See the documentation for more:
             * https://docs.abp.io/en/abp/latest/Object-Extensions
             */
        });
    }
}
