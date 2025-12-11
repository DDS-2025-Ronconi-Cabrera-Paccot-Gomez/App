using Microsoft.AspNetCore.Identity;
using Shouldly;
using System;
using System.Threading.Tasks;
using TravelPro.EntityFrameworkCore;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using Volo.Abp.Account;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Tests.Profile
{
    public class ProfileAppService_Tests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly IProfileAppService _profileAppService;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly FakeCurrentUser _fakeCurrentUser;

        public ProfileAppService_Tests()
        {
            _profileAppService = GetRequiredService<IProfileAppService>();
            _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
            _fakeCurrentUser = GetRequiredService<FakeCurrentUser>();
        }

        [Fact]
        public async Task Should_Update_Profile_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _fakeCurrentUser.SetId(userId);

            // Crear usuario real en BD
            await WithUnitOfWorkAsync(async () =>
            {
                var db = GetRequiredService<TravelProDbContext>();
                await db.Set<IdentityUser>().AddAsync(
                    new IdentityUser(userId, "testuser", "test@example.com")
                );
                await db.SaveChangesAsync();
            });

            // Leer el usuario para obtener el UserName actual (CLAVE)
            var existingUser = await _userRepository.GetAsync(userId);

            var newName = "Nuevo Nombre";
            var newEmail = "nuevo@example.com";
            var newPhoto = "foto123.png";

            // Act
            var dto = new UpdateProfileDto
            {
                Name = newName,
                Email = newEmail,
                UserName = existingUser.UserName
            };

            // Asignar propiedad extendida correctamente
            dto.ExtraProperties["ProfilePhoto"] = newPhoto;

            // Ejecutar el servicio
            await _profileAppService.UpdateAsync(dto);


            // Assert
            var user = await _userRepository.GetAsync(userId);

            user.Name.ShouldBe(newName);
            user.Email.ShouldBe(newEmail);
            user.GetProperty<string>("ProfilePhoto").ShouldBe(newPhoto);
            user.NormalizedEmail.ShouldBe(newEmail.ToUpper());
        }


        [Fact]
        public async Task Should_Not_Allow_Updating_When_Not_Authenticated()
        {
            _fakeCurrentUser.SetId(null);

            await Should.ThrowAsync<Volo.Abp.Authorization.AbpAuthorizationException>(async () =>
            {
                await _profileAppService.UpdateAsync(new UpdateProfileDto
                {
                    Name = "x",
                    Email = "x@example.com"
                });
            });
        }

        [Fact]
        public async Task Should_Not_Modify_Other_Users_Profile()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            _fakeCurrentUser.SetId(currentUserId);

            string userAUserName = "userA";
            string userBUserName = "userB";

            // Crear ambos usuarios con username y email válidos
            await WithUnitOfWorkAsync(async () =>
            {
                var db = GetRequiredService<TravelProDbContext>();

                await db.Set<IdentityUser>().AddAsync(
                    new IdentityUser(currentUserId, userAUserName, "a@example.com")
                );

                await db.Set<IdentityUser>().AddAsync(
                    new IdentityUser(anotherUserId, userBUserName, "b@example.com")
                );

                await db.SaveChangesAsync();
            });

            // Leer el usuario actual (User A) para obtener correctamente el UserName actual
            var userA_before = await _userRepository.GetAsync(currentUserId);

            // Act: actualizar solo el usuario actual
            await _profileAppService.UpdateAsync(new UpdateProfileDto
            {
                Name = "Name A Updated",
                Email = "updatedA@example.com",

                // CRÍTICO: si no copiamos el username actual, ABP intenta poner ""
                UserName = userA_before.UserName
            });

            // Assert
            var userA_after = await _userRepository.GetAsync(currentUserId);
            var userB_after = await _userRepository.GetAsync(anotherUserId);

            // User A se actualiza
            userA_after.Name.ShouldBe("Name A Updated");
            userA_after.Email.ShouldBe("updatedA@example.com");
            var userB_before = await _userRepository.GetAsync(anotherUserId);
            // User B NO se modifica
            userB_after.Name.ShouldBe(userB_before.Name);          
            userB_after.UserName.ShouldBe(userBUserName);
            userB_after.Email.ShouldBe("b@example.com");
        }





    }
}
