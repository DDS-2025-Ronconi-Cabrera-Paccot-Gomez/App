using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.GeoServices;
using TravelPro.TravelProGeo;
using Xunit;
using NSubstitute.ExceptionExtensions;

namespace TravelPro.Destinations
{
    public class CitySearchService_UnitTests
    {
        private readonly ICitySearchAPIService _citySearchServiceMock;
        private readonly CitySearchService _cityAppService;

        public CitySearchService_UnitTests()
        {
            // 1. Creamos nuestro Mock
            _citySearchServiceMock = Substitute.For<ICitySearchAPIService>();

            // 2. Creamos una instancia real de nuestro servicio, pero le pasamos el Mock
            _cityAppService = new CitySearchService(_citySearchServiceMock);
        }

        // Aquí irán nuestras pruebas...
        //Prueba 1 -Busqueda con resultados-
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnCities_WhenApiProvidesResults()
        {
            // Arrange (Preparar)
            var partialName = "Buenos";
            var fakeApiResult = new List<CitySearchResultDto>
    {
        new CitySearchResultDto { Name = "Buenos Aires", Country = "Argentina", Population = 2891000, Coordinates = new Coordinate("-34.6037", "-58.3816") },
        new CitySearchResultDto { Name = "Buenaventura", Country = "Colombia", Population = 432368, Coordinates = new Coordinate("3.8802", "-77.0275") }
    };

            // Le decimos a nuestro Mock qué debe responder
            _citySearchServiceMock
                .SearchCitiesByNameAsync(partialName)
                .Returns(Task.FromResult(fakeApiResult));

            // Act (Actuar)
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert (Verificar)
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.Items[0].Name.ShouldBe("Buenos Aires");
            result.Items[0].Country.ShouldBe("Argentina");
        }
        //Prueba 2 -Busqueda sin resultados-
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnEmptyList_WhenApiProvidesNoResults()
        {
            // Arrange
            var partialName = "CiudadInexistente";
            var emptyApiResult = new List<CitySearchResultDto>();

            _citySearchServiceMock
                .SearchCitiesByNameAsync(partialName)
                .Returns(Task.FromResult(emptyApiResult));

            // Act
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }
        //Prueba 3 -Entrada inválida-
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnEmptyList_WhenInputIsInvalid()
        {
            // Arrange
            var partialName = ""; // Input inválido/vacío

            // Act
            var result = await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();

            // Opcional: Verificar que el servicio externo NUNCA fue llamado
            await _citySearchServiceMock.DidNotReceive().SearchCitiesByNameAsync(Arg.Any<string>());
        }
        //Prueba 4 -Error simulado de la API-
        [Fact]
        public async Task SearchCitiesAsync_ShouldThrowException_WhenApiFails()
        {
            // Arrange
            var partialName = "Test";

            // Le decimos al Mock que lance un error cuando lo llamen
            _citySearchServiceMock
                .SearchCitiesByNameAsync(Arg.Any<string>())
                .Throws(new Exception("Simulated API failure!"));

            // Act & Assert
            var exception = await Should.ThrowAsync<Exception>(async () =>
            {
                await _cityAppService.SearchCitiesAsync(new SearchDestinationsInputDto { PartialName = partialName });
            });

            exception.Message.ShouldBe("Simulated API failure!");
        }
    }
 



}