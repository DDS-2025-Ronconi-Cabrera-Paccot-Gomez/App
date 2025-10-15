using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelPro.Converters;
using TravelPro.TravelProGeo;

namespace TravelPro.GeoServices
{
    public class CitySearchAPIService : ICitySearchAPIService
    {
        private readonly string apiKey = "1b87288382msh04081de1250362fp1acf94jsn6c66e7e31d14";
        private readonly string baseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";

        public async Task<List<CitySearchResultDto>> SearchCitiesByNameAsync(string cityName)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");

                string url = $"{baseUrl}/cities?namePrefix={Uri.EscapeDataString(cityName)}&limit=5"; //cambiar limite de busqueda

                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<CitySearchResultDto>();

                string json = await response.Content.ReadAsStringAsync();

                // acá podés ver el JSON crudo
                //Console.WriteLine("JSON recibido:");
                //Console.WriteLine(json);

                // Definimos una clase para mapear la raíz de la respuesta
                // Definimos opciones de serialización
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Registramos nuestro converter personalizado
                options.Converters.Add(new CitySearchResultDtoConverter());

                // Deserializamos la respuesta usando el converter
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