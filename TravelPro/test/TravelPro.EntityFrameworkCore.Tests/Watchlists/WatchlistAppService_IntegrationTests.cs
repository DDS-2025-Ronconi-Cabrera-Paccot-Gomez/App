using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Watchlists;
using TravelPro.Watchlists.Dtos;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Tests.Watchlists
{
    public class WatchlistAppService_IntegrationTests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly IWatchlistAppService _watchlistAppService;
        private readonly IRepository<Watchlist, Guid> _watchlistRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public WatchlistAppService_IntegrationTests()
        {
            _watchlistAppService = GetRequiredService<IWatchlistAppService>();
            _watchlistRepository = GetRequiredService<IRepository<Watchlist, Guid>>();
            _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
            _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
        }

        // --- HELPERS ---
        private async Task<IdentityUser> CreateUserAsync(string userName)
        {
            var user = new IdentityUser(Guid.NewGuid(), userName, $"{userName}@test.com");
            await WithUnitOfWorkAsync(async () => await _userRepository.InsertAsync(user, true));
            return user;
        }

        private async Task<Destination> CreateDestinationAsync(string name)
        {
            var dest = new Destination
            {
                Name = name,
                Country = "Test Country",
                Coordinates = new Coordinate("0", "0"),
                Photo = "",
                Region = "Region",
                LastUpdated = DateTime.Now,
                Population = 1000
            };
            await WithUnitOfWorkAsync(async () => await _destinationRepository.InsertAsync(dest, true));
            return dest;
        }

        private void Login(Guid userId)
        {
            var fakeUser = GetRequiredService<FakeCurrentUser>();
            fakeUser.SetId(userId);
        }

        // --- PRUEBA 6.1: AGREGAR A FAVORITOS ---
        [Fact]
        public async Task Should_Add_Destination_To_Watchlist()
        {
            // Arrange
            var user = await CreateUserAsync("fan1");
            var dest = await CreateDestinationAsync("Bali");
            Login(user.Id);

            // Act
            var result = await _watchlistAppService.CreateAsync(new CreateWatchlistDto
            {
                DestinationId = dest.Id
            });

            // Assert
            result.ShouldNotBeNull();
            result.DestinationName.ShouldBe("Bali"); // Verifica que enriqueció el DTO

            // CORRECCIÓN: Envolvemos la verificación de BD en una Unidad de Trabajo
            await WithUnitOfWorkAsync(async () =>
            {
                var inDb = await _watchlistRepository.FirstOrDefaultAsync(x => x.UserId == user.Id && x.DestinationId == dest.Id);
                inDb.ShouldNotBeNull();
            });
        }

        [Fact]
        public async Task Should_Prevent_Duplicate_Favorites()
        {
            // Arrange
            var user = await CreateUserAsync("fan2");
            var dest = await CreateDestinationAsync("Paris");
            Login(user.Id);

            // Agregamos la primera vez (éxito)
            await _watchlistAppService.CreateAsync(new CreateWatchlistDto { DestinationId = dest.Id });

            // Act & Assert
            // Intentamos agregar la segunda vez (debe fallar)
            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _watchlistAppService.CreateAsync(new CreateWatchlistDto { DestinationId = dest.Id });
            });
        }

        // --- PRUEBA 6.3: CONSULTAR MI LISTA ---
        [Fact]
        public async Task Should_Get_Only_My_Watchlist()
        {
            // Arrange
            var userA = await CreateUserAsync("userA");
            var userB = await CreateUserAsync("userB");

            var destA = await CreateDestinationAsync("Roma");
            var destB = await CreateDestinationAsync("Tokyo");

            // User A agrega Roma
            Login(userA.Id);
            await _watchlistAppService.CreateAsync(new CreateWatchlistDto { DestinationId = destA.Id });

            // User B agrega Tokyo
            Login(userB.Id);
            await _watchlistAppService.CreateAsync(new CreateWatchlistDto { DestinationId = destB.Id });

            // Act: Consultamos la lista siendo User A
            Login(userA.Id);
            var myList = await _watchlistAppService.GetMyWatchlistAsync();

            // Assert
            myList.Count.ShouldBe(1);
            myList.First().DestinationName.ShouldBe("Roma");
            myList.ShouldNotContain(x => x.DestinationName == "Tokyo"); // No debe ver lo de B
        }

        // --- PRUEBA 6.2: ELIMINAR DE FAVORITOS ---
        [Fact]
        public async Task Should_Remove_From_Watchlist_By_Destination()
        {
            // Arrange
            var user = await CreateUserAsync("hater");
            var dest = await CreateDestinationAsync("Cancun");
            Login(user.Id);

            // Agregamos
            await _watchlistAppService.CreateAsync(new CreateWatchlistDto { DestinationId = dest.Id });

            // Verificamos que está ahí
            (await _watchlistAppService.IsInWatchlistAsync(dest.Id)).ShouldBeTrue();

            // Act: Eliminamos por ID de destino
            await _watchlistAppService.RemoveByDestinationAsync(dest.Id);

            // Assert
            (await _watchlistAppService.IsInWatchlistAsync(dest.Id)).ShouldBeFalse();

            // CORRECCIÓN: Envolvemos la verificación de BD en una Unidad de Trabajo
            await WithUnitOfWorkAsync(async () =>
            {
                var inDb = await _watchlistRepository.FirstOrDefaultAsync(x => x.UserId == user.Id && x.DestinationId == dest.Id);
                inDb.ShouldBeNull();
            });
        }
    }
}