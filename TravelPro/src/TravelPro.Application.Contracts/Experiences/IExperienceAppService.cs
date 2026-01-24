using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Experiences.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Experiences
{
    public interface IExperienceAppService : ICrudAppService<
        ExperienceDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateExperienceDto>
    {
        // 4.4. Consultar experiencias de otros usuarios en un destino.
        Task<List<ExperienceDto>> GetListByDestinationAsync(Guid destinationId);

        // 4.5. Filtrar experiencias por valoración (positiva / negativa / neutral).
        Task<List<ExperienceDto>> GetListBySentimentAsync(Guid destinationId, ExperienceSentiment sentiment);

        // 4.6. Buscar experiencias por palabras clave (ej. “gastronomía”, “seguridad”).
        Task<List<ExperienceDto>> SearchByKeywordAsync(string keyword);
    }
}
