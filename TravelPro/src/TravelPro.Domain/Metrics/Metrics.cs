using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelPro.Metrics
{
    public class ApiMetric : CreationAuditedEntity<Guid>
    {
        public string ApiName { get; set; } // "GeoDB", "Ticketmaster"
        public string Url { get; set; }     // La URL exacta consultada
        public int StatusCode { get; set; } // 200, 404, 500
        public long DurationMs { get; set; } // Cuánto tardó en milisegundos

        protected ApiMetric() { }

        public ApiMetric(Guid id, string apiName, string url, int statusCode, long durationMs)
            : base(id)
        {
            ApiName = apiName;
            Url = url;
            StatusCode = statusCode;
            DurationMs = durationMs;
        }
    }
}