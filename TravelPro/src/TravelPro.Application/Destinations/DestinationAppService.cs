using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.Ratings;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Data;

namespace TravelPro.Destinations;
public class DestinationAppService :
    CrudAppService<
        Destination, //The Book entity
        DestinationDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateDestinationDto>, //Used to create/update a book
    IDestinationAppService,
    ICitySearchService//implement the IBookAppService

{

    private readonly IDestinationAppService _destinationAppService;
    private readonly ICitySearchService _citySearchService;
    private readonly IRepository<Rating, Guid> _ratingRepository;
    private readonly IDataFilter _dataFilter;
    public DestinationAppService(IRepository<Destination, Guid> repository, ICitySearchService citySearchService, IRepository<Rating, Guid> ratingRepository, IDataFilter dataFilter)
        : base(repository)
    {
        _citySearchService = citySearchService;
        _ratingRepository = ratingRepository;
        _dataFilter = dataFilter;
    }


    public async Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
    {

        Console.BackgroundColor = ConsoleColor.Blue;
        Console.WriteLine($" >>> [APP SERVICE] Recibida petición para: {input.PartialName}, Country: {input.Country}");
        Console.ResetColor();

        return await _citySearchService.SearchCitiesAsync(input);
    }

    public async Task<DestinationDto> SyncAsync(Guid id, CreateUpdateDestinationDto input)
    {
        // 1. Verificamos si ya existe en nuestra base de datos
        var query = await Repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query, x => x.Id == id);

        if (exists)
        {
            // Si ya existe, simplemente devolvemos la entidad mapeada (o nada, según prefieras)
            var existingEntity = await Repository.GetAsync(id);
            return ObjectMapper.Map<Destination, DestinationDto>(existingEntity);
        }

        // 2. Si NO existe, la creamos usando los datos que vienen del frontend

        var newDestination = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

        
        typeof(Volo.Abp.Domain.Entities.Entity<Guid>).GetProperty("Id")?.SetValue(newDestination, id);

        // Guardamos forzosamente en la BD
        await Repository.InsertAsync(newDestination, autoSave: true);

        return ObjectMapper.Map<Destination, DestinationDto>(newDestination);
    }
    public async Task<List<CountryDto>> GetCountriesAsync()
    {
        // Redirigimos la llamada al servicio de búsqueda inyectado
        return await _citySearchService.GetCountriesAsync();
    }

    [HttpGet("api/app/destination/regions")] // Forzamos la ruta exacta
    public async Task<List<RegionDto>> GetRegionsAsync(string countryCode)
    {
        // Delegamos al servicio de la API externa
        return await _citySearchService.GetRegionsAsync(countryCode);
    }
    public async Task<List<DestinationDto>> GetTopDestinationsAsync()
    {
        // 4. DESACTIVAMOS EL FILTRO AQUÍ
        // Esto permite leer las calificaciones de TODOS los usuarios para calcular el ranking real.
        using (_dataFilter.Disable<IUserOwned>())
        {
            // 1. Obtenemos los ratings (ahora ve todos, no solo los míos)
            var query = await _ratingRepository.GetQueryableAsync();

            var ratingsQuery = query.Select(r => new { r.DestinationId, r.Score });
            var ratingsData = await AsyncExecuter.ToListAsync(ratingsQuery);

            if (!ratingsData.Any()) return new List<DestinationDto>();

            // 3. Agrupamos y Ordenamos (En memoria)
            var sortedDestinationIds = ratingsData
                .GroupBy(r => r.DestinationId)
                .Select(g => new
                {
                    DestinationId = g.Key,
                    AverageScore = g.Average(r => r.Score),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.AverageScore)
                .ThenByDescending(x => x.Count)
                .Select(x => x.DestinationId)
                .ToList();

            // 4. Buscamos los detalles completos de esos destinos
            var destinations = await Repository.GetListAsync(d => sortedDestinationIds.Contains(d.Id));

            // 5. Reordenamos la lista final
            var finalOrderedList = sortedDestinationIds
                .Select(id => destinations.FirstOrDefault(d => d.Id == id))
                .Where(d => d != null)
                .ToList();

            return ObjectMapper.Map<List<Destination>, List<DestinationDto>>(finalOrderedList);
        }
    }

}