using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TravelPro.TravelProGeo;
using TravelPro.Destinations;


namespace TravelPro.Converters
    {
        public class CitySearchResultDtoConverter : JsonConverter<CitySearchResultDto>
        {
            public override CitySearchResultDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                {
                    var root = jsonDoc.RootElement;

                    return new CitySearchResultDto
                    {
                        Name = root.GetProperty("name").GetString(),
                        Country = root.GetProperty("country").GetString(),
                        Population = root.GetProperty("population").GetInt32(),
                        Coordinates = new Coordinate(
                            root.GetProperty("latitude").GetDouble().ToString(),
                            root.GetProperty("longitude").GetDouble().ToString()
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
                writer.WriteString("latitude", value.Coordinates.Latitude);
                writer.WriteString("longitude", value.Coordinates.Longitude);
                writer.WriteEndObject();
            }
        }
    }


