using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelPro.Events.Dtos;
using TravelPro.Metrics;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace TravelPro.Events
{
    public class EventAPIService : IEventAPIService, ITransientDependency
    {
        private readonly string apiKey = "pis6jNIILZnLRYvDksINQffc2FYx3G8C";
        private readonly string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";

        // Inyecciones
        private readonly IRepository<ApiMetric, Guid> _metricRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EventAPIService(
            IRepository<ApiMetric, Guid> metricRepository,
            IGuidGenerator guidGenerator,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _metricRepository = metricRepository;
            _guidGenerator = guidGenerator;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<List<EventDetail>> GetEventsByCityAsync(string city)
        {
            var stopwatch = Stopwatch.StartNew();
            var statusCode = 500;
            string url = $"{baseUrl}?apikey={apiKey}&city={Uri.EscapeDataString(city)}&sort=date,asc&size=3";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    statusCode = (int)response.StatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<TicketmasterResponse>(json, options);
                        return result?.Embedded?.Events ?? new List<EventDetail>();
                    }
                    return new List<EventDetail>();
                }
            }
            catch
            {
                statusCode = 500;
                return new List<EventDetail>(); // En Ticketmaster solemos devolver vacío si falla para no romper notificaciones
            }
            finally
            {
                stopwatch.Stop();
                // Guardamos en una transacción separada
                using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
                {
                    await _metricRepository.InsertAsync(new ApiMetric(
                        _guidGenerator.Create(),
                        "Ticketmaster",
                        url,
                        statusCode,
                        stopwatch.ElapsedMilliseconds
                    ));
                    await uow.CompleteAsync();
                }
            }
        }
    }
}