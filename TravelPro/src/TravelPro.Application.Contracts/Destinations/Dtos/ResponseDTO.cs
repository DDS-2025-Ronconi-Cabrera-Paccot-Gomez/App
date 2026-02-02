using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Destinations.Dtos
{

    public class CityDto: AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }
        public string Region { get; set; }

        public Coordinate Coordinates { get; set; }
}
}

