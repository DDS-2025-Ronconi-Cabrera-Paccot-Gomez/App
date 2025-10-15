using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TravelPro.Destinations;
public class DestinationAppService :
    CrudAppService<
        Destination, //The Book entity
        DestinationDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateDestinationDto>, //Used to create/update a book
    IDestinationAppService //implement the IBookAppService

{

    private readonly ICitySearchService _citySearchService;

    public DestinationAppService(IRepository<Destination, Guid> repository, ICitySearchService citySearchService)
        : base(repository)
    {
        _citySearchService = citySearchService;
    }


    public async Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
    {

        return await _citySearchService.SearchCitiesAsync(input);
    }

}