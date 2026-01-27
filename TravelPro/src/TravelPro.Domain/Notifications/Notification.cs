using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelPro.Notifications
{
    public class Notification : CreationAuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Url { get; set; } // Link al evento en Ticketmaster
        public bool IsRead { get; set; }

        protected Notification() { }

        public Notification(Guid id, Guid userId, string title, string message, string url)
            : base(id)
        {
            UserId = userId;
            Title = title;
            Message = message;
            Url = url;
            IsRead = false;
        }
    }
}