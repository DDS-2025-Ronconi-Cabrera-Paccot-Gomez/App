using System;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Notifications.Dtos
{
    public class NotificationDto : CreationAuditedEntityDto<Guid>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
    }
}