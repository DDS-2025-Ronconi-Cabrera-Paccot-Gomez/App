using System;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Destinations.Dtos
{
    public class RegionDto
    {
        public string Name { get; set; } // El nombre visible (ej: "Andalucía")
        public string Code { get; set; } // El código ISO (ej: "AN")
    }
}
