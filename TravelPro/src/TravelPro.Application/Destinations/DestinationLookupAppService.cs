using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.TravelProGeo;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using TravelPro.TravelProGeo;
using System.Linq;
namespace TravelPro.Destinations;

    public class CityAppService : ApplicationService
    {
        private readonly ITravelProService _citySearchService;

        public CityAppService(ITravelProService citySearchService)
        {
            _citySearchService = citySearchService;
        }

        public async Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
        {
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
