using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.TravelProGeo;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
namespace TravelPro.Destinations;

    public class CitySearchService : ApplicationService, ICitySearchService, ITransientDependency
{
        private readonly ICitySearchAPIService _citySearchService;

        public CitySearchService(ICitySearchAPIService citySearchService)
        {
            _citySearchService = citySearchService;
        }

        public async Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
        {
        if (string.IsNullOrWhiteSpace(input.PartialName))
        {
            return new ListResultDto<CityDto>(); // Devuelve una lista vacía inmediatamente.
        }

        var cities = await _citySearchService.SearchCitiesByNameAsync(input.PartialName);

        var result = cities.Select(c => new CityDto
        {
            Name = c.Name,
            Country = c.Country,
            Coordinates = c.Coordinates,
            Population = c.Population
        }).ToList();

            return new ListResultDto<CityDto>(result);
        }
    }
