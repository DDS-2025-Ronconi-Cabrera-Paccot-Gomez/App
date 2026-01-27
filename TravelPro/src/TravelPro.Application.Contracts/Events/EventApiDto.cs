using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TravelPro.Events.Dtos
{
    // Clase raíz de la respuesta
    public class TicketmasterResponse
    {
        [JsonPropertyName("_embedded")]
        public EmbeddedData Embedded { get; set; }
    }

    public class EmbeddedData
    {
        [JsonPropertyName("events")]
        public List<EventDetail> Events { get; set; }
    }

    public class EventDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("dates")]
        public EventDates Dates { get; set; }
    }

    public class EventDates
    {
        [JsonPropertyName("start")]
        public EventStart Start { get; set; }
    }

    public class EventStart
    {
        [JsonPropertyName("localDate")]
        public string LocalDate { get; set; }
    }
}
