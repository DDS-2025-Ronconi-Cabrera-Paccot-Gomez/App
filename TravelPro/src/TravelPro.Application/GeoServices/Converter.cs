using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.TravelProGeo;

namespace TravelPro.Converters
{
    public class CitySearchResultDtoConverter : JsonConverter<CitySearchResultDto>
    {
        public override CitySearchResultDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;

                // 1. NOMBRE: Intentamos 'name', si no 'city'
                string name = root.TryGetProperty("name", out var nameProp)
                    ? nameProp.GetString()
                    : (root.TryGetProperty("city", out var cityProp) ? cityProp.GetString() : "Desconocido");

                // 2. PAÍS: 
                string country = root.TryGetProperty("country", out var countryProp)
                    ? countryProp.GetString()
                    : ""; // Default seguro para tu prueba actual

                // 3. POBLACIÓN: 
                int population = 0;
                if (root.TryGetProperty("population", out var popProp) && popProp.ValueKind != JsonValueKind.Null)
                {
                    population = popProp.GetInt32();
                }

                // 4. REGIÓN:
                string region = root.TryGetProperty("region", out var regionProp)
                    ? regionProp.GetString()
                    : ""; // Ajuste manual o dejar vacío

                // 5. COORDENADAS: En tu JSON sí vienen
                string lat = root.TryGetProperty("latitude", out var latProp)
                    ? latProp.GetDouble().ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                    : "0";

                string lon = root.TryGetProperty("longitude", out var lonProp)
                    ? lonProp.GetDouble().ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                    : "0";
              

                return new CitySearchResultDto
                {
                    Name = name,
                    Country = country,
                    Population = population,
                    Region = region, // 
                    Coordinates = new Coordinate(lat,lon)
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, CitySearchResultDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteString("country", value.Country);
            writer.WriteNumber("population", value.Population);

            // Escribimos la región solo si tiene valor
            if (!string.IsNullOrEmpty(value.Region))
            {
                writer.WriteString("region", value.Region);
            }

            writer.WriteString("latitude", value.Coordinates.Latitude);
            writer.WriteString("longitude", value.Coordinates.Longitude);
            writer.WriteEndObject();
        }
    }
}


