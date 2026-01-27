using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Events;
using TravelPro.Events.Dtos;

namespace TravelPro.EntityFrameworkCore.Tests.Fakes
{
    public class FakeEventAPIService : IEventAPIService
    {
        public Task<List<EventDetail>> GetEventsByCityAsync(string city)
        {
            // Simulamos que SIEMPRE hay eventos en "London"
            if (city == "London")
            {
                return Task.FromResult(new List<EventDetail>
                {
                    new EventDetail
                    {
                        Name = "Concierto Fake 1",
                        Url = "http://fake1.com",
                        Dates = new EventDates { Start = new EventStart { LocalDate = "2030-01-01" } }
                    },
                    new EventDetail
                    {
                        Name = "Teatro Fake 2",
                        Url = "http://fake2.com",
                        Dates = new EventDates { Start = new EventStart { LocalDate = "2030-02-01" } }
                    }
                });
            }

            // Para otras ciudades, no hay eventos
            return Task.FromResult(new List<EventDetail>());
        }
    }
}