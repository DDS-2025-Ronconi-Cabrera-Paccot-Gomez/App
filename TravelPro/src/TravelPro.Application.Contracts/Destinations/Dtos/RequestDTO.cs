using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelPro.Destinations.Dtos
{
    public class SearchDestinationsInputDto
{
    public string PartialName { get; set; } = string.Empty;
        public int? MinPopulation { get; set; }
        public string? Country { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}