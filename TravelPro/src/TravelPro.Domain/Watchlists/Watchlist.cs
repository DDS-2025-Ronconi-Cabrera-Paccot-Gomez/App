using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelPro.Watchlists
{
    // Usamos CreationAuditedEntity porque solo nos interesa saber cuándo se agregó y por quién.
    public class Watchlist : CreationAuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid DestinationId { get; set; }

        // Constructor privado para ORM
        protected Watchlist() { }

        public Watchlist(Guid id, Guid userId, Guid destinationId)
            : base(id)
        {
            UserId = userId;
            DestinationId = destinationId;
        }
    }
}
