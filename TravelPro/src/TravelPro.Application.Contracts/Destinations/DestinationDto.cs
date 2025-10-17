using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using TravelPro.Destinations;

namespace TravelPro.Destinations;
public class DestinationDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Country { get; set; }

    public int Population { get; set; }
    public string Photo { get; set; }
    public string Region { get; set; }
    public DateTime LastUpdated { get; set; }
    public Coordinate Coordinates { get; set; }

}

