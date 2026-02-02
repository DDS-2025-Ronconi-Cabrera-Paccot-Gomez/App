using System;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Users.Dtos
{
    public class PublicProfileDto : EntityDto<Guid>
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ProfilePhoto { get; set; } // Solo la foto, nada de email ni teléfono

        // A futuro aquí podrías agregar:
        // public int ReviewsCount { get; set; }
        // public double AverageRating { get; set; }
    }
}