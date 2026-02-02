using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Ratings;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;
namespace TravelPro.EntityFrameworkCore.Tests.Ratings
{
    public class RatingAppService_IntegrationTests : TravelProApplicationTestBase<TravelProEntityFrameworkCoreTestModule>
    {
        private readonly IRatingAppService _ratingAppService;
        private readonly IRepository<Rating, Guid> _ratingRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IDestinationAppService _destinationAppService;

        public RatingAppService_IntegrationTests()
        {
            _ratingAppService = GetRequiredService<IRatingAppService>();
            _ratingRepository = GetRequiredService<IRepository<Rating, Guid>>();
            _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
            _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
            _currentUser = GetRequiredService<ICurrentUser>();
            _destinationAppService = GetRequiredService<IDestinationAppService>();
        }

        [Fact]
        public async Task Should_Require_Authentication_When_Creating_Rating()
        {
            // Arrange
            var destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
            var destination = await destinationRepository.InsertAsync(
                new Destination
                {
                    Name = "Destino no autenticado",
                    Country = "País Test",
                    Coordinates = new Coordinate("0", "0"),
                    Photo = "default_photo.png",
                    Region = "default_region",
                    LastUpdated = DateTime.Now,
                    Population = 0
                },
                autoSave: true
            );
            var dto = new CreateUpdateRatingDto
            {
                DestinationId = destination.Id,
                UserId = Guid.NewGuid(),
                Score = 5,
                Comment = "Excelente destino"
            };

            // Act & Assert
            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _ratingAppService.CreateAsync(dto)
            );
        }

        [Fact]
        public async Task Should_Create_And_Filter_Ratings_By_User()
        {
            // Arrange
            var userId = Guid.NewGuid();
            LoginAsTestUser(userId);

            await WithUnitOfWorkAsync(async () =>
            {
                var userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
                var destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();

                // Crear usuario
                var user = await userRepository.InsertAsync(
                    new IdentityUser(userId, "testuser", "test@example.com"),
                    autoSave: true
                );

                // Crear destino
                var destination = await destinationRepository.InsertAsync(
                    new Destination
                    {
                        Name = "Test Destination Duplicates",
                        Country = "País de Prueba",
                        Coordinates = new Coordinate("1", "1"),
                        Photo = "default_photo.png",
                        Region = "default_region",
                        LastUpdated = DateTime.Now,
                        Population = 0
                    },
                    autoSave: true
                );

                // Crear rating DTO
                var dto = new CreateUpdateRatingDto
                {
                    DestinationId = destination.Id,
                    UserId = user.Id,
                    Score = 4,
                    Comment = "Muy lindo lugar"
                };

                // Act
                var result = await _ratingAppService.CreateAsync(dto);

                // Assert
                result.ShouldNotBeNull();
                result.UserId.ShouldBe(user.Id);
            });

            // 🔹 Nueva unidad de trabajo para verificar los datos guardados
            await WithUnitOfWorkAsync(async () =>
            {
                var ratingRepository = GetRequiredService<IRepository<Rating, Guid>>();
                var saved = await ratingRepository.FirstOrDefaultAsync(r => r.UserId == userId);

                saved.ShouldNotBeNull();
                saved.Score.ShouldBe(4);
            });
        }


        private void LoginAsTestUser(Guid userId)
        {
            var currentUser = GetRequiredService<FakeCurrentUser>();
            currentUser.SetId(userId);
        }

        // ----------------------------------------------------------------
        // HELPER METHODS (Para crear datos rápido en los tests)
        // ----------------------------------------------------------------

        private async Task<IdentityUser> CreateUserAsync(string userName, string email)
        {
            var userId = Guid.NewGuid();
            var user = new IdentityUser(userId, userName, email);
            await WithUnitOfWorkAsync(async () =>
            {
                await _userRepository.InsertAsync(user, true);
            });
            return user;
        }

        private async Task<Destination> CreateDestinationAsync()
        {
            var dest = new Destination
            {
                Name = "Destino Test " + Guid.NewGuid().ToString().Substring(0, 5),
                Country = "Pais Test",
                Coordinates = new Coordinate("0", "0"),
                Photo = "img.png",
                Region = "Region",
                LastUpdated = DateTime.Now,
                Population = 1000
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinationRepository.InsertAsync(dest, true);
            });
            return dest;
        }

        private void Login(Guid userId)
        {
            var fakeUser = GetRequiredService<FakeCurrentUser>();
            fakeUser.SetId(userId);
        }

        // TESTS PUNTO 5.3: EDITAR Y ELIMINAR
        [Fact]
        public async Task Should_Update_Own_Rating()
        {
            // 1. Arrange
            var user = await CreateUserAsync("editor_user", "editor@abp.io");
            var dest = await CreateDestinationAsync();
            Login(user.Id);

            // Creamos la calificación original (3 estrellas)
            var rating = await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
            {
                DestinationId = dest.Id,
                Score = 3,
                Comment = "Original"
            });

            // 2. Act: Intentamos cambiarla a 5 estrellas
            var updatedRating = await _ratingAppService.UpdateAsync(rating.Id, new CreateUpdateRatingDto
            {
                DestinationId = dest.Id,
                Score = 5,
                Comment = "Editado"
            });

            // 3. Assert
            updatedRating.Score.ShouldBe(5);
            updatedRating.Comment.ShouldBe("Editado");
        }
        [Fact]
        public async Task Should_Prevent_Updating_Others_Rating()
        {
            // ----------------------------------------------------------
            // 1. ARRANGE
            // ----------------------------------------------------------

            // Creamos dos usuarios distintos
            var userA = await CreateUserAsync("user_A", "a@abp.io");
            var userB = await CreateUserAsync("user_B", "b@abp.io");

            // Creamos un destino
            var dest = await CreateDestinationAsync();

            Guid ratingId = Guid.Empty;

            // Creamos la calificación como Usuario A usando el AppService
            // (Esto asegura que el Rating quede bien persistido y con UserId correcto)
            Login(userA.Id);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _ratingAppService.CreateAsync(
                    new CreateUpdateRatingDto
                    {
                        DestinationId = dest.Id,
                        Score = 5,
                        Comment = "Soy de A y esto es legal"
                    });

                ratingId = result.Id;
            });

            // ----------------------------------------------------------
            // 2. ACT & ASSERT
            // ----------------------------------------------------------

            // Cambiamos de usuario: ahora somos User B
            Login(userB.Id);

            // ABP aplica automáticamente el filtro IUserOwned.
            // El usuario B no puede "ver" el Rating de A, por lo tanto
            // UpdateAsync debe lanzar EntityNotFoundException.
            await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    await _ratingAppService.UpdateAsync(
                        ratingId,
                        new CreateUpdateRatingDto
                        {
                            DestinationId = dest.Id,
                            Score = 1,
                            Comment = "Soy B y quiero hackear"
                        });
                });
            });
        }

        [Fact]
        public async Task Should_Delete_Own_Rating()
        {
            // 1. Arrange
            var user = await CreateUserAsync("delete_user", "del@abp.io");
            var dest = await CreateDestinationAsync();
            Login(user.Id);

            var rating = await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
            {
                DestinationId = dest.Id,
                Score = 5
            });

            // 2. Act
            await _ratingAppService.DeleteAsync(rating.Id);

            // 3. Assert
            // Verificamos directamente en el repositorio que ya no exista
            await WithUnitOfWorkAsync(async () =>
            {
                var repo = GetRequiredService<IRepository<Rating, Guid>>();
                var deletedRating = await repo.FindAsync(rating.Id);
                deletedRating.ShouldBeNull();
            });
        }

        // TEST PUNTO 5.4: ESTADÍSTICAS (PROMEDIO)

        [Fact]
        public async Task Should_Return_Correct_Average_And_Count_For_Destination()
        {
            // ----------------------------------------------------------
            // 1. ARRANGE
            // ----------------------------------------------------------

            // Creamos usuarios
            var userA = await CreateUserAsync("user_A", "a@abp.io");
            var userB = await CreateUserAsync("user_B", "b@abp.io");
            var userC = await CreateUserAsync("user_C", "c@abp.io");

            // Creamos un destino
            var destination = await CreateDestinationAsync();

            // Usuario A califica con 5
            Login(userA.Id);
            await WithUnitOfWorkAsync(async () =>
            {
                await _ratingAppService.CreateAsync(
                    new CreateUpdateRatingDto
                    {
                        DestinationId = destination.Id,
                        Score = 5,
                        Comment = "Excelente"
                    });
            });

            // Usuario B califica con 4
            Login(userB.Id);
            await WithUnitOfWorkAsync(async () =>
            {
                await _ratingAppService.CreateAsync(
                    new CreateUpdateRatingDto
                    {
                        DestinationId = destination.Id,
                        Score = 4,
                        Comment = "Muy bueno"
                    });
            });

            // Usuario C califica con 3
            Login(userC.Id);
            await WithUnitOfWorkAsync(async () =>
            {
                await _ratingAppService.CreateAsync(
                    new CreateUpdateRatingDto
                    {
                        DestinationId = destination.Id,
                        Score = 3,
                        Comment = "Normal"
                    });
            });

            // ----------------------------------------------------------
            // 2. ACT
            // ----------------------------------------------------------

            RatingStatsDto stats = null;

            await WithUnitOfWorkAsync(async () =>
            {
                stats = await _ratingAppService.GetStatsByDestinationAsync(destination.Id);
            });

            // ----------------------------------------------------------
            // 3. ASSERT
            // ----------------------------------------------------------

            stats.ShouldNotBeNull();
            stats.TotalCount.ShouldBe(3);

            // (5 + 4 + 3) / 3 = 4.0
            stats.AverageScore.ShouldBe(4.0);
        }


        // --- PUNTO 5.5: LISTAR COMENTARIOS CON NOMBRES DE USUARIO ---
        [Fact]
        public async Task Should_List_Ratings_With_UserNames()
        {
            // 1. Arrange
            var dest = await CreateDestinationAsync();
            var user = await CreateUserAsync("juan_viajero", "juan@abp.io");

            Login(user.Id);
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
            {
                DestinationId = dest.Id,
                Score = 5,
                Comment = "Lugar increible"
            });

            // 2. Act
            var list = await _ratingAppService.GetListByDestinationAsync(dest.Id);

            // 3. Assert
            list.ShouldNotBeEmpty();
            var review = list.First();

            review.Comment.ShouldBe("Lugar increible");

            // ¡Esta es la clave del 5.5! Verificar que trajo el nombre y no solo el ID
            review.UserName.ShouldBe("juan_viajero");
        }

        //Punto 3.4 - Destinos populares
        [Fact]
        public async Task Should_Return_Top_Destinations_Ordered_By_Score()
        {
            // 1. ARRANGE

            // Creamos 3 destinos
            var paris = await CreateDestinationAsync(); // Será el MEJOR (5 estrellas)
            var londres = await CreateDestinationAsync(); // Será el PEOR (3 estrellas)
            var madrid = await CreateDestinationAsync(); // Será el INTERMEDIO (4 estrellas)

            // Asignamos nombres para verificar fácil
            paris.Name = "Paris";
            londres.Name = "Londres";
            madrid.Name = "Madrid";
            await WithUnitOfWorkAsync(async () => await _destinationRepository.UpdateAsync(paris));
            await WithUnitOfWorkAsync(async () => await _destinationRepository.UpdateAsync(londres));
            await WithUnitOfWorkAsync(async () => await _destinationRepository.UpdateAsync(madrid));

            // Creamos un usuario votante
            var user = await CreateUserAsync("voter", "voter@abp.io");
            Login(user.Id);

            // Votamos
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto { DestinationId = paris.Id, Score = 5 });
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto { DestinationId = londres.Id, Score = 3 });
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto { DestinationId = madrid.Id, Score = 4 });

            // 2. ACT
            // Llamamos al método que devuelve el ranking
            // Usamos WithUnitOfWorkAsync para asegurar que lea los cambios recientes
            var topList = await _destinationAppService.GetTopDestinationsAsync();

            // 3. ASSERT
            topList.ShouldNotBeEmpty();
            topList.Count.ShouldBeGreaterThanOrEqualTo(3);

            // Verificamos el ORDEN (Descendente por puntaje)
            // #1 Debería ser Paris (5.0)
            topList[0].Name.ShouldBe("Paris");

            // #2 Debería ser Madrid (4.0)
            topList[1].Name.ShouldBe("Madrid");

            // #3 Debería ser Londres (3.0)
            topList[2].Name.ShouldBe("Londres");
        }
    }

}