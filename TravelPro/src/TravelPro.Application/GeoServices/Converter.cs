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

                // Leemos la región de forma segura (por si la API no la manda)
                string region = null;
                if (root.TryGetProperty("region", out var regionProperty) && regionProperty.ValueKind != JsonValueKind.Null)
                {
                    region = regionProperty.GetString();
                }

                return new CitySearchResultDto
                {
                    Name = root.GetProperty("name").GetString(),
                    Country = root.GetProperty("country").GetString(),
                    Population = root.GetProperty("population").GetInt32(),
                    Region = region, // <--- Aquí asignamos el nuevo campo mapeado
                    Coordinates = new Coordinate(
                        root.GetProperty("latitude").GetDouble().ToString("R", CultureInfo.InvariantCulture),
                        root.GetProperty("longitude").GetDouble().ToString("R", CultureInfo.InvariantCulture)
                    )
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


