using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Destinations;
public interface IDestinationAppService :
    ICrudAppService< //Defines CRUD methods
        DestinationDto, //Used to show Destinations
        Guid, //Primary key of the Destination entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateDestinationDto> //Used to create/update a book
{
    Task<DestinationDto> SyncAsync(Guid id, CreateUpdateDestinationDto input);
}