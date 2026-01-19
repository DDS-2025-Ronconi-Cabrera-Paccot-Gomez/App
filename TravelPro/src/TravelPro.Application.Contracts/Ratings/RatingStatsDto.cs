using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelPro.Ratings;

public class RatingStatsDto
{
    public double AverageScore { get; set; } // El promedio (ej. 4.5)
    public int TotalCount { get; set; }      // Cantidad de votos
}
