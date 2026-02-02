using System;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Experiences.Dtos
{
    public class ExperienceDto : AuditedEntityDto<Guid>
    {
        public Guid DestinationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ExperienceSentiment Sentiment { get; set; }
        public decimal Cost { get; set; }
        public DateTime ExperienceDate { get; set; }
        public string Tags { get; set; } // Ej: "comida, barato, noche"

        // Propiedad extra para mostrar el autor en la UI
        public string UserName { get; set; }
    }
}
