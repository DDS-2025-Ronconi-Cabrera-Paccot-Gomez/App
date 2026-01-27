using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPro.Watchlists.Dtos
{
    public class CreateWatchlistDto
    {
        [Required]
        public Guid DestinationId { get; set; }
    }
}