using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TravelPro.Destinations;
using TravelPro.Watchlists.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Mvc;

namespace TravelPro.Watchlists
{
    [Authorize] // Todo requiere login
    public class WatchlistAppService : TravelProAppService, IWatchlistAppService
    {
        private readonly IRepository<Watchlist, Guid> _watchlistRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;

        public WatchlistAppService(
            IRepository<Watchlist, Guid> watchlistRepository,
            IRepository<Destination, Guid> destinationRepository)
        {
            _watchlistRepository = watchlistRepository;
            _destinationRepository = destinationRepository;
        }

        // 6.1 AGREGAR A FAVORITOS
        public async Task<WatchlistDto> CreateAsync(CreateWatchlistDto input)
        {
            var userId = CurrentUser.GetId();

            // 1. Validar que no exista ya (Evitar duplicados)
            var exists = await _watchlistRepository.AnyAsync(x => x.UserId == userId && x.DestinationId == input.DestinationId);
            if (exists)
            {
                throw new UserFriendlyException("Este destino ya está en tu lista de seguimiento.");
            }

            // 2. Validar que el destino exista
            var destination = await _destinationRepository.FindAsync(input.DestinationId);
            if (destination == null)
            {
                throw new UserFriendlyException("El destino no existe.");
            }

            // 3. Guardar
            var watchlist = new Watchlist(Guid.NewGuid(), userId, input.DestinationId);
            await _watchlistRepository.InsertAsync(watchlist, autoSave: true);

            // 4. Retornar DTO enriquecido
            var dto = ObjectMapper.Map<Watchlist, WatchlistDto>(watchlist);
            dto.DestinationName = destination.Name;
            dto.DestinationCountry = destination.Country;

            return dto;
        }

        // 6.2 ELIMINAR (Por ID de Watchlist)
        public async Task DeleteAsync(Guid id)
        {
            var item = await _watchlistRepository.GetAsync(id);

            if (item.UserId != CurrentUser.GetId())
            {
                throw new UserFriendlyException("No tienes permiso para eliminar este ítem.");
            }

            await _watchlistRepository.DeleteAsync(item);
        }

        // 6.2 ELIMINAR (Por ID de Destino - Helper para UI)
        [HttpDelete("api/app/watchlist/by-destination")]
        public async Task RemoveByDestinationAsync(Guid destinationId)
        {
            var userId = CurrentUser.GetId();

            var item = await _watchlistRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.DestinationId == destinationId);

            if (item != null)
            {
                await _watchlistRepository.DeleteAsync(item);
            }
        }

        // 6.3 CONSULTAR MI LISTA
        public async Task<List<WatchlistDto>> GetMyWatchlistAsync()
        {
            var userId = CurrentUser.GetId();

            // Obtenemos la lista del usuario
            var query = await _watchlistRepository.GetQueryableAsync();
            var items = await AsyncExecuter.ToListAsync(query.Where(x => x.UserId == userId));

            if (!items.Any()) return new List<WatchlistDto>();

            // Enriquecemos con datos del destino (Nombre, País)
            var destIds = items.Select(x => x.DestinationId).Distinct().ToList();
            var destinations = await _destinationRepository.GetListAsync(d => destIds.Contains(d.Id));

            var dtos = ObjectMapper.Map<List<Watchlist>, List<WatchlistDto>>(items);

            foreach (var dto in dtos)
            {
                var dest = destinations.FirstOrDefault(d => d.Id == dto.DestinationId);
                if (dest != null)
                {
                    dto.DestinationName = dest.Name;
                    dto.DestinationCountry = dest.Country;
                   
                }
            }

            return dtos;
        }

        // EXTRA: SABER SI YA ES FAVORITO
        [HttpGet("api/app/watchlist/is-in-watchlist")]
        public async Task<bool> IsInWatchlistAsync(Guid destinationId)
        {
            if (CurrentUser.Id == null) return false;
            return await _watchlistRepository.AnyAsync(x => x.UserId == CurrentUser.Id && x.DestinationId == destinationId);
        }
    }
}