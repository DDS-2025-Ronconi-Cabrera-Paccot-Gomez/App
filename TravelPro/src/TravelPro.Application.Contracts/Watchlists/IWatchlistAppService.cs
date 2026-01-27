using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Watchlists.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Watchlists
{
    public interface IWatchlistAppService : IApplicationService
    {
        // 6.1 Agregar
        Task<WatchlistDto> CreateAsync(CreateWatchlistDto input);

        // 6.2 Eliminar (por el ID de la tabla Watchlist)
        Task DeleteAsync(Guid id);

        // 6.2 Eliminar (por el ID del Destino - Más práctico para el botón)
        Task RemoveByDestinationAsync(Guid destinationId);

        // 6.3 Consultar mi lista
        Task<List<WatchlistDto>> GetMyWatchlistAsync();

        // Extra: Verificar si un destino ya está en mi lista (para pintar el corazón)
        Task<bool> IsInWatchlistAsync(Guid destinationId);
    }
}