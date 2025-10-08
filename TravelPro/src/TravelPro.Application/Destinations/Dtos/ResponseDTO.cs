using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelPro.Destinations.Dtos
{

    public class CityDto
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }

        public Coordinate Coordinates { get; set; }
}
}

