using System;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Destinations
{
    public interface ICitySearchService : IApplicationService
    {
        Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input);
       
    }
}