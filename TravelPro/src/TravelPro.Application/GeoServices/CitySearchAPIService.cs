using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelPro.Converters;
using TravelPro.Destinations.Dtos;
using TravelPro.TravelProGeo;
using Volo.Abp;

namespace TravelPro.GeoServices
{
    public class CitySearchAPIService : ICitySearchAPIService
    {
        private readonly string apiKey = "1b87288382msh04081de1250362fp1acf94jsn6c66e7e31d14";
        private readonly string baseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";
        private readonly string apiHost = "wft-geo-db.p.rapidapi.com";

        public async Task<List<CitySearchResultDto>> SearchCitiesAsync(SearchDestinationsInputDto input)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

                // --- 1. RESOLUCIÓN DE PAÍS ROBUSTA ---
                string countryId = null;

                // Limpiamos espacios (Trim) para evitar errores tontos
                var cleanCountryInput = input.Country?.Trim();

                if (!string.IsNullOrWhiteSpace(cleanCountryInput))
                {
                    if (cleanCountryInput.Length == 2)
                    {
                        // Es un código (ej: "AR")
                        countryId = cleanCountryInput.ToUpper();
                    }
                    else
                    {
                        // Es un nombre (ej: "Argentina") -> Intentamos buscar su código
                        countryId = await GetCountryCodeByNameAsync(client, cleanCountryInput);
                    }
                }

                string regionCode = null;
                var cleanRegion = input.Region?.Trim();
                if (!string.IsNullOrWhiteSpace(cleanRegion) && !string.IsNullOrEmpty(countryId))
                {
                    if (cleanRegion.Length <= 3 && cleanRegion.All(char.IsUpper))
                    {
                        // Si es código corto, lo usamos directo
                        regionCode = cleanRegion;
                    }
                    else
                    {
                        // BUSCAMOS MÚLTIPLES REGIONES (Fuzzy)
                        // Si escriben "Castilla", traerá códigos de "Castilla y León" y "Castilla-La Mancha"
                        var codes = await GetRegionCodesByNameAsync(client, countryId, cleanRegion);

                        if (codes.Any())
                        {
                            // Unimos los códigos con comas (ej: "CL,CM")
                            regionCode = string.Join(",", codes);
                            Console.WriteLine($"[GEO SEARCH] Región '{cleanRegion}' mapeada a: {regionCode}");
                        }
                    }
                }
                // --- 2. CONSTRUCCIÓN DE LA URL ---
                string url;
                string queryParams = $"?limit=10&sort=-population";

                if (!string.IsNullOrWhiteSpace(input.PartialName))
                    queryParams += $"&namePrefix={Uri.EscapeDataString(input.PartialName)}";


               if (input.MinPopulation.HasValue)
                 queryParams += $"&minPopulation={input.MinPopulation.Value}";

                // AQUÍ SE APLICA EL FILTRO
                if (!string.IsNullOrEmpty(countryId) &&
                    !string.IsNullOrEmpty(regionCode) &&
                    !regionCode.Contains(","))
                {
                    url = $"{baseUrl}/countries/{countryId}/regions/{regionCode}/cities{queryParams}";
                    //queryParams += $"&countryIds={countryId}";
                }

                else
                {
                    if (!string.IsNullOrEmpty(countryId))
                        queryParams += $"&countryIds={countryId}";

                    // Nota: Aquí mantenemos 'regionCodes' por si acaso cae en el else
                    if (!string.IsNullOrEmpty(regionCode))
                        queryParams += $"&regionCodes={Uri.EscapeDataString(regionCode)}";
                    else if (!string.IsNullOrWhiteSpace(input.Region) && input.Region.Length <= 3)
                        queryParams += $"&regionCodes={Uri.EscapeDataString(input.Region.Trim().ToUpper())}";

                    url = $"{baseUrl}/cities{queryParams}";
                }




                // --- 4. EJECUCIÓN ---
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<CitySearchResultDto>();

                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new CitySearchResultDtoConverter());

                var apiResponse = JsonSerializer.Deserialize<GeoDbResponse>(json, options);
 
            
                var cities = apiResponse?.Data ?? new List<CitySearchResultDto>();

               
                foreach (var city in cities)
                {
                    if (string.IsNullOrWhiteSpace(city.Country))
                    {
                        city.Country = input.Country; 
                    }

                    if (string.IsNullOrWhiteSpace(city.Region))
                    {
                        city.Region = input.Region; 
                    }
                }

               
                return cities;
            }
        }
        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            var countries = new List<CountryDto>
            {
                new CountryDto { Code = "AF", Name = "Afganistán" },
                new CountryDto { Code = "AL", Name = "Albania" },
                new CountryDto { Code = "DE", Name = "Alemania" },
                new CountryDto { Code = "AD", Name = "Andorra" },
                new CountryDto { Code = "AO", Name = "Angola" },
                new CountryDto { Code = "AI", Name = "Anguila" },
                new CountryDto { Code = "AQ", Name = "Antártida" },
                new CountryDto { Code = "AG", Name = "Antigua y Barbuda" },
                new CountryDto { Code = "SA", Name = "Arabia Saudita" },
                new CountryDto { Code = "DZ", Name = "Argelia" },
                new CountryDto { Code = "AR", Name = "Argentina" },
                new CountryDto { Code = "AM", Name = "Armenia" },
                new CountryDto { Code = "AW", Name = "Aruba" },
                new CountryDto { Code = "AU", Name = "Australia" },
                new CountryDto { Code = "AT", Name = "Austria" },
                new CountryDto { Code = "AZ", Name = "Azerbaiyán" },
                new CountryDto { Code = "BS", Name = "Bahamas" },
                new CountryDto { Code = "BH", Name = "Baréin" },
                new CountryDto { Code = "BD", Name = "Bangladesh" },
                new CountryDto { Code = "BB", Name = "Barbados" },
                new CountryDto { Code = "BE", Name = "Bélgica" },
                new CountryDto { Code = "BZ", Name = "Belice" },
                new CountryDto { Code = "BJ", Name = "Benín" },
                new CountryDto { Code = "BM", Name = "Bermudas" },
                new CountryDto { Code = "BY", Name = "Bielorrusia" },
                new CountryDto { Code = "BO", Name = "Bolivia" },
                new CountryDto { Code = "BA", Name = "Bosnia y Herzegovina" },
                new CountryDto { Code = "BW", Name = "Botsuana" },
                new CountryDto { Code = "BR", Name = "Brasil" },
                new CountryDto { Code = "BN", Name = "Brunéi" },
                new CountryDto { Code = "BG", Name = "Bulgaria" },
                new CountryDto { Code = "BF", Name = "Burkina Faso" },
                new CountryDto { Code = "BI", Name = "Burundi" },
                new CountryDto { Code = "BT", Name = "Bután" },
                new CountryDto { Code = "CV", Name = "Cabo Verde" },
                new CountryDto { Code = "KH", Name = "Camboya" },
                new CountryDto { Code = "CM", Name = "Camerún" },
                new CountryDto { Code = "CA", Name = "Canadá" },
                new CountryDto { Code = "QA", Name = "Catar" },
                new CountryDto { Code = "TD", Name = "Chad" },
                new CountryDto { Code = "CL", Name = "Chile" },
                new CountryDto { Code = "CN", Name = "China" },
                new CountryDto { Code = "CY", Name = "Chipre" },
                new CountryDto { Code = "CO", Name = "Colombia" },
                new CountryDto { Code = "KM", Name = "Comoras" },
                new CountryDto { Code = "KP", Name = "Corea del Norte" },
                new CountryDto { Code = "KR", Name = "Corea del Sur" },
                new CountryDto { Code = "CI", Name = "Costa de Marfil" },
                new CountryDto { Code = "CR", Name = "Costa Rica" },
                new CountryDto { Code = "HR", Name = "Croacia" },
                new CountryDto { Code = "CU", Name = "Cuba" },
                new CountryDto { Code = "DK", Name = "Dinamarca" },
                new CountryDto { Code = "DM", Name = "Dominica" },
                new CountryDto { Code = "EC", Name = "Ecuador" },
                new CountryDto { Code = "EG", Name = "Egipto" },
                new CountryDto { Code = "SV", Name = "El Salvador" },
                new CountryDto { Code = "AE", Name = "Emiratos Árabes Unidos" },
                new CountryDto { Code = "ER", Name = "Eritrea" },
                new CountryDto { Code = "SK", Name = "Eslovaquia" },
                new CountryDto { Code = "SI", Name = "Eslovenia" },
                new CountryDto { Code = "ES", Name = "España" },
                new CountryDto { Code = "US", Name = "Estados Unidos" },
                new CountryDto { Code = "EE", Name = "Estonia" },
                new CountryDto { Code = "ET", Name = "Etiopía" },
                new CountryDto { Code = "PH", Name = "Filipinas" },
                new CountryDto { Code = "FI", Name = "Finlandia" },
                new CountryDto { Code = "FJ", Name = "Fiyi" },
                new CountryDto { Code = "FR", Name = "Francia" },
                new CountryDto { Code = "GA", Name = "Gabón" },
                new CountryDto { Code = "GM", Name = "Gambia" },
                new CountryDto { Code = "GE", Name = "Georgia" },
                new CountryDto { Code = "GH", Name = "Ghana" },
                new CountryDto { Code = "GD", Name = "Granada" },
                new CountryDto { Code = "GR", Name = "Grecia" },
                new CountryDto { Code = "GT", Name = "Guatemala" },
                new CountryDto { Code = "GN", Name = "Guinea" },
                new CountryDto { Code = "GQ", Name = "Guinea Ecuatorial" },
                new CountryDto { Code = "GY", Name = "Guyana" },
                new CountryDto { Code = "HT", Name = "Haití" },
                new CountryDto { Code = "HN", Name = "Honduras" },
                new CountryDto { Code = "HU", Name = "Hungría" },
                new CountryDto { Code = "IN", Name = "India" },
                new CountryDto { Code = "ID", Name = "Indonesia" },
                new CountryDto { Code = "IQ", Name = "Irak" },
                new CountryDto { Code = "IR", Name = "Irán" },
                new CountryDto { Code = "IE", Name = "Irlanda" },
                new CountryDto { Code = "IS", Name = "Islandia" },
                new CountryDto { Code = "IL", Name = "Israel" },
                new CountryDto { Code = "IT", Name = "Italia" },
                new CountryDto { Code = "JM", Name = "Jamaica" },
                new CountryDto { Code = "JP", Name = "Japón" },
                new CountryDto { Code = "JO", Name = "Jordania" },
                new CountryDto { Code = "KZ", Name = "Kazajistán" },
                new CountryDto { Code = "KE", Name = "Kenia" },
                new CountryDto { Code = "KG", Name = "Kirguistán" },
                new CountryDto { Code = "KW", Name = "Kuwait" },
                new CountryDto { Code = "LA", Name = "Laos" },
                new CountryDto { Code = "LS", Name = "Lesoto" },
                new CountryDto { Code = "LV", Name = "Letonia" },
                new CountryDto { Code = "LB", Name = "Líbano" },
                new CountryDto { Code = "LR", Name = "Liberia" },
                new CountryDto { Code = "LY", Name = "Libia" },
                new CountryDto { Code = "LI", Name = "Liechtenstein" },
                new CountryDto { Code = "LT", Name = "Lituania" },
                new CountryDto { Code = "LU", Name = "Luxemburgo" },
                new CountryDto { Code = "MK", Name = "Macedonia del Norte" },
                new CountryDto { Code = "MG", Name = "Madagascar" },
                new CountryDto { Code = "MY", Name = "Malasia" },
                new CountryDto { Code = "MW", Name = "Malaui" },
                new CountryDto { Code = "MV", Name = "Maldivas" },
                new CountryDto { Code = "ML", Name = "Malí" },
                new CountryDto { Code = "MT", Name = "Malta" },
                new CountryDto { Code = "MA", Name = "Marruecos" },
                new CountryDto { Code = "MU", Name = "Mauricio" },
                new CountryDto { Code = "MR", Name = "Mauritania" },
                new CountryDto { Code = "MX", Name = "México" },
                new CountryDto { Code = "FM", Name = "Micronesia" },
                new CountryDto { Code = "MD", Name = "Moldavia" },
                new CountryDto { Code = "MC", Name = "Mónaco" },
                new CountryDto { Code = "MN", Name = "Mongolia" },
                new CountryDto { Code = "ME", Name = "Montenegro" },
                new CountryDto { Code = "MZ", Name = "Mozambique" },
                new CountryDto { Code = "MM", Name = "Myanmar" },
                new CountryDto { Code = "NA", Name = "Namibia" },
                new CountryDto { Code = "NR", Name = "Nauru" },
                new CountryDto { Code = "NP", Name = "Nepal" },
                new CountryDto { Code = "NI", Name = "Nicaragua" },
                new CountryDto { Code = "NE", Name = "Níger" },
                new CountryDto { Code = "NG", Name = "Nigeria" },
                new CountryDto { Code = "NO", Name = "Noruega" },
                new CountryDto { Code = "NZ", Name = "Nueva Zelanda" },
                new CountryDto { Code = "OM", Name = "Omán" },
                new CountryDto { Code = "NL", Name = "Países Bajos" },
                new CountryDto { Code = "PK", Name = "Pakistán" },
                new CountryDto { Code = "PW", Name = "Palaos" },
                new CountryDto { Code = "PA", Name = "Panamá" },
                new CountryDto { Code = "PG", Name = "Papúa Nueva Guinea" },
                new CountryDto { Code = "PY", Name = "Paraguay" },
                new CountryDto { Code = "PE", Name = "Perú" },
                new CountryDto { Code = "PL", Name = "Polonia" },
                new CountryDto { Code = "PT", Name = "Portugal" },
                new CountryDto { Code = "GB", Name = "Reino Unido" },
                new CountryDto { Code = "CF", Name = "República Centroafricana" },
                new CountryDto { Code = "CZ", Name = "República Checa" },
                new CountryDto { Code = "DO", Name = "República Dominicana" },
                new CountryDto { Code = "RW", Name = "Ruanda" },
                new CountryDto { Code = "RO", Name = "Rumania" },
                new CountryDto { Code = "RU", Name = "Rusia" },
                new CountryDto { Code = "WS", Name = "Samoa" },
                new CountryDto { Code = "KN", Name = "San Cristóbal y Nieves" },
                new CountryDto { Code = "SM", Name = "San Marino" },
                new CountryDto { Code = "VC", Name = "San Vicente y las Granadinas" },
                new CountryDto { Code = "LC", Name = "Santa Lucía" },
                new CountryDto { Code = "SN", Name = "Senegal" },
                new CountryDto { Code = "RS", Name = "Serbia" },
                new CountryDto { Code = "SC", Name = "Seychelles" },
                new CountryDto { Code = "SL", Name = "Sierra Leona" },
                new CountryDto { Code = "SG", Name = "Singapur" },
                new CountryDto { Code = "SY", Name = "Siria" },
                new CountryDto { Code = "SO", Name = "Somalia" },
                new CountryDto { Code = "LK", Name = "Sri Lanka" },
                new CountryDto { Code = "ZA", Name = "Sudáfrica" },
                new CountryDto { Code = "SD", Name = "Sudán" },
                new CountryDto { Code = "SE", Name = "Suecia" },
                new CountryDto { Code = "CH", Name = "Suiza" },
                new CountryDto { Code = "SR", Name = "Surinam" },
                new CountryDto { Code = "TH", Name = "Tailandia" },
                new CountryDto { Code = "TW", Name = "Taiwán" },
                new CountryDto { Code = "TZ", Name = "Tanzania" },
                new CountryDto { Code = "TJ", Name = "Tayikistán" },
                new CountryDto { Code = "TL", Name = "Timor Oriental" },
                new CountryDto { Code = "TG", Name = "Togo" },
                new CountryDto { Code = "TO", Name = "Tonga" },
                new CountryDto { Code = "TT", Name = "Trinidad y Tobago" },
                new CountryDto { Code = "TN", Name = "Túnez" },
                new CountryDto { Code = "TM", Name = "Turkmenistán" },
                new CountryDto { Code = "TR", Name = "Turquía" },
                new CountryDto { Code = "TV", Name = "Tuvalu" },
                new CountryDto { Code = "UA", Name = "Ucrania" },
                new CountryDto { Code = "UG", Name = "Uganda" },
                new CountryDto { Code = "UY", Name = "Uruguay" },
                new CountryDto { Code = "UZ", Name = "Uzbekistán" },
                new CountryDto { Code = "VU", Name = "Vanuatu" },
                new CountryDto { Code = "VE", Name = "Venezuela" },
                new CountryDto { Code = "VN", Name = "Vietnam" },
                new CountryDto { Code = "YE", Name = "Yemen" },
                new CountryDto { Code = "ZM", Name = "Zambia" },
                new CountryDto { Code = "ZW", Name = "Zimbabue" }
            };

            // Ordenamos alfabéticamente para que se vea bien en el select
            return await Task.FromResult(countries.OrderBy(c => c.Name).ToList());
        }

        private async Task<string> GetCountryCodeByNameAsync(HttpClient client, string countryName)
        {
            try
            {
                string url = $"{baseUrl}/countries?namePrefix={Uri.EscapeDataString(countryName)}&limit=1";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(json))
                {
                    var data = doc.RootElement.GetProperty("data");
                    if (data.GetArrayLength() > 0)
                    {
                        return data[0].GetProperty("code").GetString();
                    }
                }
            }
            catch { }
            return null;
        }

        private async Task<List<string>> GetRegionCodesByNameAsync(HttpClient client, string countryCode, string regionName)
        {
            var codes = new List<string>();
            try
            {
                // Pedimos hasta 5 coincidencias (ej: "Castilla" -> Castilla y León, Castilla-La Mancha...)
                string url = $"{baseUrl}/countries/{countryCode}/regions?namePrefix={Uri.EscapeDataString(regionName)}&limit=5";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return codes;

                var json = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(json))
                {
                    var data = doc.RootElement.GetProperty("data");
                    foreach (var element in data.EnumerateArray())
                    {
                        // Intentamos obtener isoCode
                        if (element.TryGetProperty("isoCode", out var isoProp) && isoProp.ValueKind != JsonValueKind.Null)
                        {
                            codes.Add(isoProp.GetString());
                        }
                    }
                }
            }
            catch { }
            return codes;
        }

        public async Task<List<RegionDto>> GetRegionsAsync(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode)) return new List<RegionDto>();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

                // Llamamos al endpoint de regiones de un país
                string url = $"{baseUrl}/countries/{countryCode}/regions?limit=10";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<RegionDto>();

                var json = await response.Content.ReadAsStringAsync();
                var regions = new List<RegionDto>();

                using (var doc = JsonDocument.Parse(json))
                {
                    var data = doc.RootElement.GetProperty("data");
                    foreach (var element in data.EnumerateArray())
                    {
                        regions.Add(new RegionDto
                        {
                            // GeoDB usa 'isoCode' o 'name'
                            Code = element.TryGetProperty("isoCode", out var iso) ? iso.GetString() : "",
                            Name = element.GetProperty("name").GetString()
                        });
                    }
                }
                return regions.OrderBy(r => r.Name).ToList();
            }
        }
        // Clase auxiliar para mapear la respuesta de la API
        private class GeoDbResponse
        {
            public List<CitySearchResultDto> Data { get; set; }
        }
    }
}