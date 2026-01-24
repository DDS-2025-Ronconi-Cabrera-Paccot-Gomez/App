using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.GeoServices;
using TravelPro.TravelProGeo;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace TravelPro.Destinations
{
    public class CitySearchService_UnitTests
    {
        private readonly ICitySearchAPIService _citySearchServiceMock;
        private readonly IRepository<Destination, Guid> _destinationRepositoryMock;
        private readonly CitySearchService _cityAppService;

        public CitySearchService_UnitTests()
        {
            // 1. Creamos nuestros Mocks
            _citySearchServiceMock = Substitute.For<ICitySearchAPIService>();
            _destinationRepositoryMock = Substitute.For<IRepository<Destination, Guid>>();

            // 2. Creamos una instancia real de nuestro servicio pasando los Mocks
            _cityAppService = new CitySearchService(_citySearchServiceMock, _destinationRepositoryMock);
        }

        // Prueba 1 - Búsqueda con resultados
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnCities_WhenApiProvidesResults()
        {
            // Arrange
            var partialName = "Buenos";
            var fakeApiResult = new List<CitySearchResultDto>
            {
                new CitySearchResultDto { Name = "Buenos Aires", Country = "Argentina", Population = 2891000, Coordinates = new Coordinate("-34.6037", "-58.3816") },
                new CitySearchResultDto { Name = "Buenaventura", Country = "Colombia", Population = 432368, Coordinates = new Coordinate("3.8802", "-77.0275") }
            };

            // Configurar Mock de API: Esperamos un DTO cuyo PartialName sea "Buenos"
            _citySearchServiceMock
                .SearchCitiesAsync(Arg.Is<SearchDestinationsInputDto>(input => input.PartialName == partialName))
                .Returns(Task.FromResult(fakeApiResult));

            // Configurar Mock de Repositorio: Devolver lista vacía (sin coincidencias locales)
            _destinationRepositoryMock
                .GetListAsync(Arg.Any<Expression<Func<Destination, bool>>>())
                .Returns(Task.FromResult(new List<Destination>()));

            // Act
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.Items[0].Name.ShouldBe("Buenos Aires");
            result.Items[0].Country.ShouldBe("Argentina");
        }

        // Prueba 2 - Búsqueda sin resultados
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnEmptyList_WhenApiProvidesNoResults()
        {
            // Arrange
            var partialName = "CiudadInexistente";
            var emptyApiResult = new List<CitySearchResultDto>();

            _citySearchServiceMock
                .SearchCitiesAsync(Arg.Is<SearchDestinationsInputDto>(input => input.PartialName == partialName))
                .Returns(Task.FromResult(emptyApiResult));

            _destinationRepositoryMock
                .GetListAsync(Arg.Any<Expression<Func<Destination, bool>>>())
                .Returns(Task.FromResult(new List<Destination>()));

            // Act
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }

        // Prueba 3 - Entrada inválida (Vacía)
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnEmptyList_WhenInputIsInvalid()
        {
            // Arrange
            var partialName = ""; // Input inválido/vacío

            // Act
            // Enviamos un input vacío. El servicio debería validar esto al principio y retornar vacío sin llamar a la API.
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();

            // Verificamos que el servicio externo NUNCA fue llamado
            await _citySearchServiceMock.DidNotReceive().SearchCitiesAsync(Arg.Any<SearchDestinationsInputDto>());
        }

        // Prueba 4 - Error simulado de la API
        [Fact]
        public async Task SearchCitiesAsync_ShouldThrowException_WhenApiFails()
        {
            // Arrange
            var partialName = "Test";

            // Le decimos al Mock que lance un error cuando lo llamen con cualquier input válido
            _citySearchServiceMock
                .SearchCitiesAsync(Arg.Any<SearchDestinationsInputDto>())
                .Throws(new Exception("Simulated API failure!"));

            // Act & Assert
            var exception = await Should.ThrowAsync<Exception>(async () =>
            {
                await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });
            });

            exception.Message.ShouldBe("Simulated API failure!");
        }

        [Fact]
        public async Task SearchCitiesAsync_ShouldPassAllFiltersToApiService()
        {
            // 1. ARRANGE (Preparar)
            // Simulamos que el usuario busca ciudades en "Argentina" (AR), 
            // Región "Córdoba", con más de 50,000 habitantes.
            var input = new SearchDestinationsInputDto
            {
                PartialName = "Villa", // Busca ciudades que empiecen con Villa
                Country = "AR",
                Region = "Córdoba",
                MinPopulation = 50000
            };

            var fakeApiResult = new List<CitySearchResultDto>
            {
                new CitySearchResultDto
                {
                    Name = "Villa Carlos Paz",
                    Country = "Argentina",
                    Population = 62000,
                    Region = "Córdoba",
                    Coordinates = new Coordinate("-31.4", "-64.5")
                }
            };

            // CONFIGURAMOS EL MOCK DE LA API:
            // Usamos Arg.Is(...) para verificar que el servicio recibe EXACTAMENTE los filtros que pusimos.
            // Si el servicio interno modificara o borrara algún filtro, esta prueba fallaría.
            _citySearchServiceMock
                .SearchCitiesAsync(Arg.Is<SearchDestinationsInputDto>(x =>
                    x.PartialName == "Villa" &&
                    x.Country == "AR" &&
                    x.Region == "Córdoba" &&
                    x.MinPopulation == 50000
                ))
                .Returns(Task.FromResult(fakeApiResult));

            // Configuramos el repo para que devuelva lista vacía (sin coincidencias locales por ahora)
            _destinationRepositoryMock
                .GetListAsync(Arg.Any<Expression<Func<Destination, bool>>>())
                .Returns(Task.FromResult(new List<Destination>()));

            // 2. ACT (Actuar)
            var result = await _cityAppService.SearchCitiesAsync(input);

            // 3. ASSERT (Verificar)
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items[0].Name.ShouldBe("Villa Carlos Paz");

            // Verificación doble: Aseguramos que el método del mock fue llamado 1 vez con esos argumentos
            await _citySearchServiceMock.Received(1).SearchCitiesAsync(Arg.Is<SearchDestinationsInputDto>(x =>
                x.Country == "AR" && x.MinPopulation == 50000));
        }
    }
}
