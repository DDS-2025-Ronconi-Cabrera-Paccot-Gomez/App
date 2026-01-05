using Microsoft.AspNetCore.Identity;
using Shouldly;
using System;
using System.Threading.Tasks;
using TravelPro.EntityFrameworkCore;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Users;
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
        [Fact]
        public async Task Should_Delete_Own_Account()
        {
            // 1. ARRANGE (Preparar)
            var userId = Guid.NewGuid();
            _fakeCurrentUser.SetId(userId); // Simulamos que somos este usuario

            // Insertamos el usuario en la BD
            await WithUnitOfWorkAsync(async () =>
            {
                var db = GetRequiredService<TravelProDbContext>();
                await db.Set<IdentityUser>().AddAsync(
                    new IdentityUser(userId, "userToDelete", "delete@example.com")
                );
                await db.SaveChangesAsync();
            });

            // Verificamos que existe antes de borrar
            var userBefore = await _userRepository.FindAsync(userId);
            userBefore.ShouldNotBeNull();

            // 2. ACT (Actuar)
            // TRUCO: Como _profileAppService es del tipo estándar de ABP, 
            // necesitamos castearlo a TU interfaz para ver el método DeleteAsync.
            var myService = _profileAppService as ITravelProProfileAppService;

            // Si por alguna razón el cast falla, lanzamos error para avisar
            myService.ShouldNotBeNull("El servicio no implementa ITravelProProfileAppService");

            await myService.DeleteAsync();

            // 3. ASSERT (Verificar)
            // Intentamos buscarlo de nuevo. 
            // Los repositorios de ABP filtran automáticamente los "Soft Deleted", 
            // por lo que debería devolver null.
            var userAfter = await _userRepository.FindAsync(userId);
            userAfter.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Search_Users_By_Filter()
        {
            // 1. ARRANGE (Preparar el escenario con varios usuarios)

            // Creamos 3 usuarios: 2 que se llaman "Valentin" y 1 que se llama "Maria"
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var user3Id = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                var db = GetRequiredService<TravelProDbContext>();

                // Usuario 1: Coincide en el Nombre
                var u1 = new IdentityUser(user1Id, "valen_gamer", "v1@example.com");
                u1.Name = "Valentin";
                u1.Surname = "Perez";

                // Usuario 2: Coincide en el Apellido (para probar que el filtro es inteligente)
                var u2 = new IdentityUser(user2Id, "otro_user", "v2@example.com");
                u2.Name = "Juan";
                u2.Surname = "Valentin";

                // Usuario 3: NO tiene nada que ver
                var u3 = new IdentityUser(user3Id, "maria_travel", "m@example.com");
                u3.Name = "Maria";
                u3.Surname = "Gomez";

                await db.Set<IdentityUser>().AddRangeAsync(u1, u2, u3);
                await db.SaveChangesAsync();
            });

            // 2. ACT (Ejecutar la búsqueda)

            // Casteamos a tu interfaz para ver el método nuevo 'SearchAsync'
            var myService = _profileAppService as ITravelProProfileAppService;

            // Buscamos "Valentin" (debería traer al 1 y al 2)
            var resultsFound = await myService.SearchAsync("Valentin");

            // Buscamos algo que no existe
            var resultsEmpty = await myService.SearchAsync("XylophonoInexistente");

            // 3. ASSERT (Verificar)

            // Verificación A: Resultados encontrados
            resultsFound.ShouldNotBeNull();
            resultsFound.Count.ShouldBe(2); // Debería encontrar a Valentin Perez y a Juan Valentin

            // Verificamos que Maria NO esté en la lista
            resultsFound.ShouldContain(u => u.UserName == "valen_gamer");
            resultsFound.ShouldContain(u => u.UserName == "otro_user");
            resultsFound.ShouldNotContain(u => u.UserName == "maria_travel");

            // Verificación B: Búsqueda vacía
            resultsEmpty.ShouldNotBeNull();
            resultsEmpty.ShouldBeEmpty(); // La lista debe estar vacía, no ser null
        }




    }



}
