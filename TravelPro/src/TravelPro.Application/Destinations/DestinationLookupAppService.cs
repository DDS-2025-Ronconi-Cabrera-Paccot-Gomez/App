using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.TravelProGeo;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories; // Necesario para el Repositorio

namespace TravelPro.Destinations
{
    public class CitySearchService : ApplicationService, ICitySearchService, ITransientDependency
    {
        private readonly ICitySearchAPIService _citySearchService;
        private readonly IRepository<Destination, Guid> _destinationRepository; // 1. Repositorio Local

        public CitySearchService(
            ICitySearchAPIService citySearchService,
            IRepository<Destination, Guid> destinationRepository) // Inyección
        {
            _citySearchService = citySearchService;
            _destinationRepository = destinationRepository;
        }

        public async Task<ListResultDto<CityDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.PartialName))
            {
                return new ListResultDto<CityDto>();
            }

            // A) Traer ciudades de la API Externa
            var apiCities = await _citySearchService.SearchCitiesAsync(input);

            // B) Mapeo inicial a DTOs (inicialmente con ID vacío)
            var resultDtos = apiCities.Select(c => new CityDto
            {
                Id = Guid.Empty, // Por defecto asumimos que es nueva
                Name = c.Name,
                Country = c.Country,
                Coordinates = c.Coordinates,
                Population = c.Population,
                Region = c.Region 
            }).ToList();

            // C) LÓGICA DE CRUCE: Buscar coincidencias en la BD Local

            // 1. Extraemos los nombres para filtrar rápido
            var cityNames = resultDtos.Select(x => x.Name).Distinct().ToList();

            // 2. Buscamos en BD destinos que tengan esos nombres
            // (Traemos una lista candidata para no hacer 10 queries separadas)
            var existingDestinations = await _destinationRepository.GetListAsync(d => cityNames.Contains(d.Name));

            // 3. Asignamos los IDs correctos
            foreach (var dto in resultDtos)
            {
                // Buscamos si existe una coincidencia exacta de Nombre y País
                var match = existingDestinations.FirstOrDefault(d =>
                    d.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase) &&
                    d.Country.Equals(dto.Country, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    // ¡Encontramos la ciudad en nuestra BD! Usamos su ID real.
                    dto.Id = match.Id;
                }
                // Si match es null, el ID se queda en Guid.Empty (0000...)
            }

            return new ListResultDto<CityDto>(resultDtos);
        }
        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            // Delegamos la llamada al servicio de infraestructura (API Externa)
            return await _citySearchService.GetCountriesAsync();
        }
    }
}