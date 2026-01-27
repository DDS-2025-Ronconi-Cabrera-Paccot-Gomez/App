using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Events.Dtos;

namespace TravelPro.Events
{
    public interface IEventAPIService
    {
        Task<List<EventDetail>> GetEventsByCityAsync(string city);
    }
}
