using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TravelPro.Events.Dtos;
using Volo.Abp.DependencyInjection;

namespace TravelPro.Events
{
    public class EventAPIService : IEventAPIService, ITransientDependency
    {
        private readonly string apiKey = "pis6jNIILZnLRYvDksINQffc2FYx3G8C";
        private readonly string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";

        public async Task<List<EventDetail>> GetEventsByCityAsync(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                // Construimos la URL
                string url = $"{baseUrl}?apikey={apiKey}&city={Uri.EscapeDataString(city)}&sort=date,asc&size=3";

                // LOG 1: Ver qué URL estamos intentando llamar
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"[TICKETMASTER] Solicitando: {url}");
                Console.ResetColor();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // LOG 2: Ver el código de estado (200, 401, 404?)
                    Console.WriteLine($"[TICKETMASTER] Respuesta: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        // LOG 3: Ver un pedacito del JSON para asegurar que llegó algo
                        // (Imprimimos los primeros 200 caracteres para no ensuciar toda la consola)
                        string jsonPreview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                        Console.WriteLine($"[TICKETMASTER] JSON Recibido: {jsonPreview}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<TicketmasterResponse>(json, options);

                        var eventos = result?.Embedded?.Events ?? new List<EventDetail>();
                        Console.WriteLine($"[TICKETMASTER] Eventos deserializados: {eventos.Count}");

                        return eventos;
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[TICKETMASTER ERROR] {errorBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TICKETMASTER EXCEPTION] {ex.Message}");
                }

                return new List<EventDetail>();
            }
        }
    }
}