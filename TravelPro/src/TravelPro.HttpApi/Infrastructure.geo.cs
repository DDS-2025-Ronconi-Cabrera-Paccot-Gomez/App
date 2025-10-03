using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelPro.TravelProGeo;

namespace TestGeodbCities.Infrastructure
{
    public class CitySearchService : ITravelProService
    {
        private readonly string apiKey = "1b87288382msh04081de1250362fp1acf94jsn6c66e7e31d14";
        private readonly string baseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";

        public async Task<List<CitySearchResultDto>> SearchCitiesByNameAsync(string cityName)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");

                string url = $"{baseUrl}/cities?namePrefix={Uri.EscapeDataString(cityName)}&limit=5";

                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<CitySearchResultDto>();

                string json = await response.Content.ReadAsStringAsync();

                // Definimos una clase para mapear la raíz de la respuesta
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var apiResponse = JsonSerializer.Deserialize<GeoDbResponse>(json, options);

                return apiResponse?.Data ?? new List<CitySearchResultDto>();
            }
        }

        // Clase auxiliar para mapear la respuesta de la API
        private class GeoDbResponse
        {
            public List<CitySearchResultDto> Data { get; set; }
        }
    }
}