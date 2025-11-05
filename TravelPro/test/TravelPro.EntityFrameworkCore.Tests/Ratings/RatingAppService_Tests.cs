using Shouldly;
using System;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Ratings;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
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

        public RatingAppService_IntegrationTests()
        {
            _ratingAppService = GetRequiredService<IRatingAppService>();
            _ratingRepository = GetRequiredService<IRepository<Rating, Guid>>();
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
    }
}