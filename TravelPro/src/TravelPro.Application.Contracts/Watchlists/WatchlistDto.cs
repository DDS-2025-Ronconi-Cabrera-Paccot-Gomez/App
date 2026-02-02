using System;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Watchlists.Dtos
{
    public class WatchlistDto : CreationAuditedEntityDto<Guid>
    {
        public Guid DestinationId { get; set; }
        public Guid UserId { get; set; }

        // Propiedades extra para mostrar en la lista sin hacer otra consulta
        public string DestinationName { get; set; }
        public string DestinationCountry { get; set; }
    }
}
