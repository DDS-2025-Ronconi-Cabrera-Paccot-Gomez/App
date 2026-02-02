using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPro.Experiences.Dtos
{
    public class CreateUpdateExperienceDto
    {
        [Required]
        public Guid DestinationId { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public ExperienceSentiment Sentiment { get; set; } // 0=Neutral, 1=Positiva, 2=Negativa

        public decimal Cost { get; set; }

        [Required]
        public DateTime ExperienceDate { get; set; }

        [StringLength(256)]
        public string Tags { get; set; } // Tags separados por coma
    }
}
