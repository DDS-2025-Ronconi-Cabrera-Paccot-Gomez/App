using System;
using System.Collections.Generic;
using TravelPro.Experiences;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelPro.Experiences
{
    public class Experience : FullAuditedEntity<Guid>
    {
        public Guid DestinationId { get; set; } // Ciudad asociada
        public Guid UserId { get; set; }
        public string Title { get; set; }       // Ej: "Cena inolvidable en el centro"
        public string Description { get; set; } // El relato completo

        public ExperienceSentiment Sentiment { get; set; } // Positiva/Negativa/Neutral

        public decimal Cost { get; set; }       // Opcional: Cuánto gastó
        public DateTime ExperienceDate { get; set; } // Cuándo ocurrió

        // Para el punto 4.6 (Palabras clave), podemos guardar un string simple
        // o una lista. Para simplificar la búsqueda en SQL, un string de tags separados por coma funciona bien.
        public string Tags { get; set; } // Ej: "gastronomía, cena, barato"

        private Experience() { }

        public Experience(Guid id, Guid destinationId, string title, string description, ExperienceSentiment sentiment)
            : base(id)
        {
            DestinationId = destinationId;
            Title = title;
            Description = description;
            Sentiment = sentiment;
        }
    }
}
