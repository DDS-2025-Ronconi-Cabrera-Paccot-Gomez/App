using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Ratings;
using Volo.Abp; // Necesario para BusinessException
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
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
            _testUserId = Guid.NewGuid();
        }

        [Fact]
        public async Task Should_Throw_BusinessException_When_Duplicate_Rating_Is_Inserted()
        {
            // --- ARRANGE ---

            // 1. Creamos el Usuario en la BD de prueba (para la Foreign Key)
            await WithUnitOfWorkAsync(async () =>
            {
                await _userRepository.InsertAsync(
                    new IdentityUser(_testUserId, "testuser_dup", "testuser_dup@abp.io"),
                    true
                );
            });

            // 🔹 Logueamos al usuario de prueba
            var fakeUser = GetRequiredService<FakeCurrentUser>();
            fakeUser.SetId(_testUserId);

            // 2. Creamos un Destino real y válido (para la Foreign Key)
            var destination = await _destinationRepository.InsertAsync(
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
                true
            );

            // 3. Creamos el primer rating (funciona)
            await _ratingAppService.CreateAsync(new CreateUpdateRatingDto
            {
                DestinationId = destination.Id,
                Score = 5
            });

            // 4. Preparamos el segundo rating (el duplicado)
            var inputDto = new CreateUpdateRatingDto
            {
                DestinationId = destination.Id, // Mismo destino
                Score = 4
            };

            // --- ACT & ASSERT ---

            // 5. Verificamos que se lance la EXCEPCIÓN DE NEGOCIO correcta
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _ratingAppService.CreateAsync(inputDto);
            });

            // 6. Verificamos el CÓDIGO de error específico
            exception.Code.ShouldBe("TravelPro:DuplicateRating");
        }
    }
}