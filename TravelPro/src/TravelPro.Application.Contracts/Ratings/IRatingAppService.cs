using System;
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

}
