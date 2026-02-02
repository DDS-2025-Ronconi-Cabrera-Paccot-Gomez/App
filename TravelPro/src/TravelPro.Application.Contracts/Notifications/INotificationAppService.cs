using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Notifications.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        // 7.2: Revisar eventos para mis favoritos
        Task<string> CheckEventsForMyWatchlistAsync();

        // 7.4: Obtener mis notificaciones
        Task<List<NotificationDto>> GetMyNotificationsAsync();

        // 7.4: Marcar una como leída
        Task MarkAsReadAsync(Guid id);

        // Extra: Marcar todas como leídas
        Task MarkAllAsReadAsync();
    }
}