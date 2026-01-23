using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Microsoft.AspNetCore.Mvc;

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
    public DestinationAppService(IRepository<Destination, Guid> repository, ICitySearchService citySearchService)
        : base(repository)
    {
        _citySearchService = citySearchService;
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
        // NOTA: Es importante crear la entidad forzando el ID que viene de la API externa
        // para que coincida con lo que tiene el frontend.

        var newDestination = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

        // HACK: En ABP, a veces el ID es protected. Usamos reflexión o constructor para forzarlo.
        // Si tu entidad Destination tiene un constructor que acepta ID, úsalo: new Destination(id)
        // Si hereda de Entity<Guid>, puedes intentar asignarlo directamente si el set es público.
        // Aquí asumimos que podemos asignar el ID mediante reflexión si es protected:
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

}