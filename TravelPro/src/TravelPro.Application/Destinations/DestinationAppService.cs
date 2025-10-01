using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public DestinationAppService(IRepository<Destination, Guid> repository)
        : base(repository)
    {

    }
}
