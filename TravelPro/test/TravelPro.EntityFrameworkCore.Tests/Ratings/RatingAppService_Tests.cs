using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore;
using TravelPro.Ratings;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Testing; // ESTE ES EL USING QUE FALTA
using Volo.Abp.Users;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Tests.Ratings
{
    public class RatingAppService_Tests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly IRatingAppService _ratingAppService;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly Guid _testUserId;

        public RatingAppService_Tests()
        {
            _ratingAppService = GetRequiredService<IRatingAppService>();
            _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
            _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
            _currentUser = GetRequiredService<ICurrentUser>();
            _testUserId = _currentUser.Id.Value;
        }

        // ... (Tu prueba Should_Throw_BusinessException_When_Duplicate_Rating_Is_Inserted va aquí) ...

        // --- PRUEBA DE INTEGRACIÓN: Filtro por Usuario (IUserOwned) ---
        [Fact]
        public async Task GetListAsync_Should_Only_Return_Ratings_For_Current_User()
        {
            // --- ARRANGE ---

            // 1. Definimos los IDs de los usuarios
            var userA_Id = _testUserId; // El usuario logueado por defecto en las pruebas
            var userB_Id = Guid.NewGuid();

            // 2. Creamos un Destino
            var destination = await _destinationRepository.InsertAsync(
                new Destination
                {
                    Name = "Test Destination Filter",
                    Country = "País de Prueba",
                    Coordinates = new Coordinate("1", "1"),
                    Photo = "default_photo.png",
                    Region = "default_region",
                    LastUpdated = DateTime.Now,
                    Population = 0
                },
                true
            );

            // 3. Creamos un Usuario A y B en la BD (para la Foreign Key)
            await WithUnitOfWorkAsync(async () =>
            {
                await _userRepository.InsertAsync(
                    new IdentityUser(userA_Id, "userA_filter", "userA_filter@abp.io"),
                    true
                );
                await _userRepository.InsertAsync(
                    new IdentityUser(userB_Id, "userB_filter", "userB_filter@abp.io"),
                    true
                );
            });


            // 4. Creamos un rating como Usuario A (el usuario actual)
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
            {
                DestinationId = destination.Id,
                Score = 5,
                Comment = "Rating de User A"
            });

            // 5. SIMULAMOS ser el Usuario B
            using (_currentUser.Change(userB_Id)) // <<--- ESTO ES LO QUE ARREGLA EL ERROR
            {
                // 6. Creamos un rating como Usuario B
                await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
                {
                    DestinationId = destination.Id,
                    Score = 1,
                    Comment = "Rating de User B"
                });
            }
            // (Al salir del 'using', volvemos a ser el Usuario A por defecto)

            // --- ACT ---

            // 7. Pedimos la lista de ratings (como Usuario A)
            var result = await _ratingAppService.GetListAsync(new PagedAndSortedResultRequestDto());

            // --- ASSERT ---

            // 8. Verificamos que solo vemos 1 resultado (el de User A)
            result.TotalCount.ShouldBe(1);
            result.Items.First().UserId.ShouldBe(userA_Id);
            result.Items.First().Comment.ShouldBe("Rating de User A");
        }
    }
}