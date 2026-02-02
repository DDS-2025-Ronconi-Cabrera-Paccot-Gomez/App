using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Ratings;

public interface IRatingAppService :
    ICrudAppService< //Defines CRUD methods
        RatingDto, //Used to show ratings
        Guid, //Primary key of the rating entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateRatingDto> //Used to create/update a rating
{
    // 5.4: Consultar promedio
    Task<RatingStatsDto> GetStatsByDestinationAsync(Guid destinationId);

    // 5.5: Listar comentarios de un destino específico con nombres de usuario
    Task<List<RatingDto>> GetListByDestinationAsync(Guid destinationId);
}
