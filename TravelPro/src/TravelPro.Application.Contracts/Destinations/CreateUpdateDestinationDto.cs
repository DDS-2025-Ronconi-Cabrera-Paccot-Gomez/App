using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TravelPro.Destinations;
public class CreateUpdateDestinationDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Coordinate Coordinates { get; set; } 

    [Required]
    [DataType(DataType.Date)]
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    [Required]
    [StringLength(128)]
    public string Region { get; set; } = string.Empty;
    
    [Required]
    [StringLength(128)]
    public string Country { get; set; } = string.Empty;

    [StringLength(128)]
    public string Photo { get; set; } = string.Empty;

    [Required]
    public int Population { get; set; }

}