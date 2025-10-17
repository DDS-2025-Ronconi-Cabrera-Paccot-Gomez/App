using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations.Dtos;
using TravelPro.EntityFrameworkCore;
using TravelPro.GeoServices;
using TravelPro.TravelProGeo;
using Xunit;

namespace TravelPro.Destinations
{
    public class CitySearchService_IntegrationTests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly ICitySearchService _cityAppService; // O tu interfaz correcta

        public CitySearchService_IntegrationTests()
        {
            // Obtenemos la implementación REAL del servicio desde el contenedor de DI
            _cityAppService = GetRequiredService<ICitySearchService>();
        }

        // Aquí irán las pruebas de integración...
        //Prueba 1 -Recibir resultados reales de la API-
        [Trait("Category", "Integration")]
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnRealResults_FromLiveApi()
        {
            // Arrange
            // Usamos un nombre de ciudad que sabemos que existe
            var input = new SearchDestinationsInputDto { PartialName = "London" };

            // Act
            var result = await _cityAppService.SearchCitiesAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldNotBeEmpty(); // Deberíamos recibir al menos un resultado
            result.Items.First().Name.ShouldContain("London");
            result.Items.First().Country.ShouldNotBeNullOrEmpty();
        }
        //Prueba 2 -Que los dtos se mapean correctamente-
        [Trait("Category", "Integration")]
        [Fact]
        public async Task SearchCitiesAsync_ShouldMapDtoCorrectly_WhenApiProvidesResults()
        {
            // Arrange
            // Usamos un nombre específico que nos dará un resultado predecible
            var input = new SearchDestinationsInputDto { PartialName = "Buenos Aires" };

            // Act
            var result = await _cityAppService.SearchCitiesAsync(input);

            // Assert
            result.Items.ShouldNotBeEmpty();

            // Buscamos el resultado específico que nos interesa
            var buenosAires = result.Items.FirstOrDefault(c => c.Name == "Buenos Aires");

            // Verificamos que encontramos la ciudad
            buenosAires.ShouldNotBeNull();

            // Ahora, verificamos campo por campo que el mapeo fue correcto
            buenosAires.Name.ShouldBe("Buenos Aires");
            buenosAires.Country.ShouldBe("Argentina");
            buenosAires.Population.ShouldBeGreaterThan(100000); // Verificamos que la población se mapeó
            buenosAires.Coordinates.ShouldNotBeNull();
            buenosAires.Coordinates.Latitude.ShouldNotBeNullOrWhiteSpace();
            buenosAires.Coordinates.Longitude.ShouldNotBeNullOrWhiteSpace();
        }
        //Prueba 3 -Manejar errores de red o respuestas inesperadas-
        [Trait("Category", "Integration")]
        [Fact]
        public async Task SearchCitiesAsync_ShouldReturnEmptyList_WhenApiReturnsError()
        {
            // Arrange
            // Esta prueba espera que la API Key en appsettings.json sea inválida
            var input = new SearchDestinationsInputDto { PartialName = "CualquierCiudad" };

            // Act
            var result = await _cityAppService.SearchCitiesAsync(input);

            // Assert
            // Verificamos que el servicio manejó el error de la API
            // y devolvió una lista vacía, en lugar de lanzar una excepción.
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }
    }
}
