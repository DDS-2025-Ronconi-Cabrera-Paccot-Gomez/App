using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using TravelPro.Notifications;
using TravelPro.Watchlists;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Tests.Notifications
{
    public class NotificationAppService_IntegrationTests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly INotificationAppService _notificationAppService;
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IRepository<Watchlist, Guid> _watchlistRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public NotificationAppService_IntegrationTests()
        {
            _notificationAppService = GetRequiredService<INotificationAppService>();
            _notificationRepository = GetRequiredService<IRepository<Notification, Guid>>();
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

        private void Login(Guid userId)
        {
            var fakeUser = GetRequiredService<FakeCurrentUser>();
            fakeUser.SetId(userId);
        }

        // --- PRUEBA 7.2: GENERAR NOTIFICACIONES ---
        [Fact]
        public async Task Should_Generate_Notifications_For_Watchlist_Events()
        {
            // 1. Arrange
            var user = await CreateUserAsync("fan_eventos");
            Login(user.Id);

            // Creamos destino "London" (que nuestro Fake sabe responder)
            var london = new Destination { Name = "London", Country = "UK", Coordinates = new Coordinate("0", "0"), Photo ="", Region = ""};
            await WithUnitOfWorkAsync(async () => await _destinationRepository.InsertAsync(london, true));

            // Agregamos London a Watchlist del usuario
            await WithUnitOfWorkAsync(async () =>
            {
                await _watchlistRepository.InsertAsync(new Watchlist(Guid.NewGuid(), user.Id, london.Id), true);
            });

            // 2. Act
            // Llamamos al método que busca eventos en la API (Fake) y crea notificaciones
            await _notificationAppService.CheckEventsForMyWatchlistAsync();

            // 3. Assert
            await WithUnitOfWorkAsync(async () =>
            {
                // Deberían haberse creado 2 notificaciones (según nuestro Fake)
                var notifs = await _notificationRepository.GetListAsync(n => n.UserId == user.Id);

                notifs.Count.ShouldBe(2);
                notifs.ShouldContain(n => n.Title.Contains("Concierto Fake 1"));
                notifs.ShouldContain(n => n.Title.Contains("Teatro Fake 2"));

                // Deben estar como NO leídas
                notifs.All(n => !n.IsRead).ShouldBeTrue();
            });
        }

        [Fact]
        public async Task Should_Not_Duplicate_Notifications_If_Checked_Twice()
        {
            // 1. Arrange
            var user = await CreateUserAsync("fan_duplicados");
            Login(user.Id);

            var london = new Destination { Name = "London", Country = "UK", Coordinates = new Coordinate("0", "0"), Photo = "", Region = "" };
            await WithUnitOfWorkAsync(async () => await _destinationRepository.InsertAsync(london, true));
            await WithUnitOfWorkAsync(async () => await _watchlistRepository.InsertAsync(new Watchlist(Guid.NewGuid(), user.Id, london.Id), true));

            // 2. Act
            // Ejecutamos la primera vez
            await _notificationAppService.CheckEventsForMyWatchlistAsync();

            // Ejecutamos la segunda vez (misma ciudad, mismos eventos del fake)
            await _notificationAppService.CheckEventsForMyWatchlistAsync();

            // 3. Assert
            await WithUnitOfWorkAsync(async () =>
            {
                // Siguen siendo 2, no 4. El sistema detectó duplicados.
                var count = await _notificationRepository.CountAsync(n => n.UserId == user.Id);
                count.ShouldBe(2);
            });
        }

        // --- PRUEBA 7.4: MARCAR COMO LEÍDA ---
        [Fact]
        public async Task Should_Mark_Notification_As_Read()
        {
            // 1. Arrange
            var user = await CreateUserAsync("reader");
            Login(user.Id);

            var notif = new Notification(Guid.NewGuid(), user.Id, "Test Title", "Msg", "Url");
            await WithUnitOfWorkAsync(async () => await _notificationRepository.InsertAsync(notif, true));

            // 2. Act
            await _notificationAppService.MarkAsReadAsync(notif.Id);

            // 3. Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var updated = await _notificationRepository.GetAsync(notif.Id);
                updated.IsRead.ShouldBeTrue();
            });
        }

        // --- PRUEBA 7.4: LISTAR MIS NOTIFICACIONES ---
        [Fact]
        public async Task Should_Get_My_Notifications_Sorted_By_Date()
        {
            // 1. Arrange
            var user = await CreateUserAsync("lister");
            Login(user.Id);

            // Insertamos 2 notificaciones
            await WithUnitOfWorkAsync(async () =>
            {
                await _notificationRepository.InsertAsync(new Notification(Guid.NewGuid(), user.Id, "Old", "Msg", ""), true);
                // Esperamos un milisegundo para asegurar diferencia de tiempo si la BD es muy rápida
                await Task.Delay(10);
                await _notificationRepository.InsertAsync(new Notification(Guid.NewGuid(), user.Id, "New", "Msg", ""), true);
            });

            // 2. Act
            var dtos = await _notificationAppService.GetMyNotificationsAsync();

            // 3. Assert
            dtos.Count.ShouldBe(2);
            // La más nueva primero (OrderByDescending CreationTime)
            dtos[0].Title.ShouldBe("New");
            dtos[1].Title.ShouldBe("Old");
        }
    }
}