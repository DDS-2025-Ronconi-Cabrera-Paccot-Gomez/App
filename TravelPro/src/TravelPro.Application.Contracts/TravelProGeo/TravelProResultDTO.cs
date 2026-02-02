using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations;

namespace TravelPro.TravelProGeo
{
    // Define los datos mínimos que te importan del resultado de la API
    public class CitySearchResultDto  
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }

        public string Region { get; set; }
        public Coordinate Coordinates { get; set; }

    }
}
