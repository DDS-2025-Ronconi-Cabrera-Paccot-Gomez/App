using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; // Para HttpPost
using TravelPro.Destinations; // Para repositorio de Destinos
using TravelPro.Events;       // Para EventAPIService
using TravelPro.Notifications.Dtos;
using TravelPro.Watchlists;   // Para repositorio de Watchlist
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TravelPro.Notifications
{
    [Authorize]
    public class NotificationAppService : ApplicationService, INotificationAppService
    {
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IRepository<Watchlist, Guid> _watchlistRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IEventAPIService _eventApiService;
  

        public NotificationAppService(
            IRepository<Notification, Guid> notificationRepository,
            IRepository<Watchlist, Guid> watchlistRepository,
            IRepository<Destination, Guid> destinationRepository,
            IEventAPIService eventApiService
            )
        {
            _notificationRepository = notificationRepository;
            _watchlistRepository = watchlistRepository;
            _destinationRepository = destinationRepository;
            _eventApiService = eventApiService;
        
        }

        // --- 7.2: GENERAR NOTIFICACIONES DE EVENTOS ---
        // Este método busca en TU Watchlist, consulta la API y crea notificaciones si hay eventos nuevos.
        [HttpPost("api/app/notifications/check-events")]
        public async Task<string> CheckEventsForMyWatchlistAsync()
        {
            var userId = CurrentUser.GetId();

            // 1. Obtener Watchlist
            var watchlistItems = await _watchlistRepository.GetListAsync(w => w.UserId == userId);
            if (!watchlistItems.Any()) return "Tu lista de favoritos está vacía.";

            var destinationIds = watchlistItems.Select(w => w.DestinationId).ToList();
            var destinations = await _destinationRepository.GetListAsync(d => destinationIds.Contains(d.Id));

            int newNotifications = 0;
            int totalEventsFound = 0;

            foreach (var dest in destinations)
            {
                // 2. Consultar API
                // IMPORTANTE: Busca por el nombre exacto que tienes en BD. 
                // Si tienes "London", funcionará. Si tienes "New York", funcionará.
                var events = await _eventApiService.GetEventsByCityAsync(dest.Name);
                totalEventsFound += events.Count;

                foreach (var evt in events)
                {
                    // VALIDACIÓN DE SEGURIDAD PARA DATOS NULOS O LARGOS

                    // A) Recortar Título (Max 128 chars aprox)
                    string rawTitle = $"Evento en {dest.Name}: {evt.Name}";
                    string safeTitle = rawTitle.Length > 100 ? rawTitle.Substring(0, 97) + "..." : rawTitle;

                    // B) Manejar Fecha Nula de forma segura
                    string dateStr = evt.Dates?.Start?.LocalDate ?? "Fecha a confirmar";
                    string rawMessage = $"Fecha: {dateStr}. ¡No te lo pierdas!";
                    string safeMessage = rawMessage.Length > 200 ? rawMessage.Substring(0, 197) + "..." : rawMessage;

                    // C) Validar URL nula
                    string safeUrl = evt.Url ?? "#";

                    // D) Verificar duplicados
                    // Usamos el título seguro para buscar
                    var exists = await _notificationRepository.AnyAsync(n =>
                        n.UserId == userId &&
                        n.Title == safeTitle);

                    if (!exists)
                    {
                        var notif = new Notification(
                            Guid.NewGuid(),
                            userId,
                            safeTitle,   // Usamos la versión recortada
                            safeMessage, // Usamos la versión recortada y segura
                            safeUrl
                        );

                        await _notificationRepository.InsertAsync(notif);
                        newNotifications++;
                    }
                }
            }

            return $"Proceso finalizado. Ciudades revisadas: {destinations.Count}. Eventos encontrados en API: {totalEventsFound}. Notificaciones nuevas creadas: {newNotifications}.";
        }
        // --- 7.4: LISTAR NOTIFICACIONES ---
        [HttpGet("api/app/notifications/my-notifications")]
        public async Task<List<NotificationDto>> GetMyNotificationsAsync()
        {
            var userId = CurrentUser.GetId();
            var notifs = await _notificationRepository.GetListAsync(n => n.UserId == userId);

            return ObjectMapper.Map<List<Notification>, List<NotificationDto>>(
                notifs.OrderByDescending(n => n.CreationTime).ToList()
            );
        }

        // --- 7.4: MARCAR COMO LEÍDA ---
        [HttpPut("api/app/notifications/{id}/read")]
        public async Task MarkAsReadAsync(Guid id)
        {
            var notif = await _notificationRepository.GetAsync(id);
            if (notif.UserId != CurrentUser.GetId())
            {
                throw new UserFriendlyException("No tienes permiso.");
            }

            notif.IsRead = true;
            await _notificationRepository.UpdateAsync(notif);
        }

        // Helper: Marcar todas como leídas
        [HttpPut("api/app/notifications/read-all")]
        public async Task MarkAllAsReadAsync()
        {
            var userId = CurrentUser.GetId();
            var notifs = await _notificationRepository.GetListAsync(n => n.UserId == userId && !n.IsRead);

            foreach (var n in notifs)
            {
                n.IsRead = true;
                await _notificationRepository.UpdateAsync(n);
            }
        }
    }
}