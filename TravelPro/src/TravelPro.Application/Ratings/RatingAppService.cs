using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using System.Linq;
using Volo.Abp.Data;


namespace TravelPro.Ratings;


[Authorize]
public class RatingAppService :
    CrudAppService<
        Rating,
        RatingDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateRatingDto>,
    IRatingAppService
{
    private readonly IObjectMapper _objectMapper;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IDataFilter _dataFilter;

    public RatingAppService(IRepository<Rating, Guid> repository, IObjectMapper objectMapper, IRepository<IdentityUser, Guid> userRepository, IDataFilter dataFilter)
        : base(repository)
    {
        _objectMapper = objectMapper;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
    }

    public override async Task<RatingDto> CreateAsync(CreateUpdateRatingDto input)
    {
        if (CurrentUser?.Id == null)
        {
            throw new AbpAuthorizationException("User must be logged in to create a rating.");
        }
        var rating = _objectMapper.Map<CreateUpdateRatingDto, Rating>(input);

        rating.UserId = CurrentUser.Id.Value;

        var existingRating = await Repository.FirstOrDefaultAsync(
            r => r.UserId == rating.UserId && r.DestinationId == rating.DestinationId
        );

        if (existingRating != null)
        {
            throw new BusinessException("TravelPro:DuplicateRating")
                .WithData("UserId", rating.UserId)
                .WithData("DestinationId", rating.DestinationId);
        }

        await Repository.InsertAsync(rating, autoSave: true);
        return _objectMapper.Map<Rating, RatingDto>(rating);
    }

    //EDITAR
    public override async Task<RatingDto> UpdateAsync(Guid id, CreateUpdateRatingDto input)
    {
        var rating = await Repository.GetAsync(id);

        if (rating.UserId != CurrentUser.Id)
        {
            throw new UserFriendlyException("No tienes permiso para editar esta calificación.");
        }

        // Actualizamos campos
        rating.Score = input.Score;
        rating.Comment = input.Comment;

        await Repository.UpdateAsync(rating);

        return await MapToGetOutputDtoAsync(rating);
    }

    //ELIMINAR
    public override async Task DeleteAsync(Guid id)
    {
        var rating = await Repository.GetAsync(id);

        if (rating.UserId != CurrentUser.Id)
        {
            throw new UserFriendlyException("No tienes permiso para eliminar esta calificación.");
        }

        await Repository.DeleteAsync(rating);
    }
    // --- PUNTO 5.4: PROMEDIO Y ESTADÍSTICAS ---
    [AllowAnonymous] // Permitir ver stats sin loguearse
    public async Task<RatingStatsDto> GetStatsByDestinationAsync(Guid destinationId)
    {
        using (DataFilter.Disable<IUserOwned>())
        {

            var query = await Repository.GetQueryableAsync();
        var ratingsOfDest = query.Where(r => r.DestinationId == destinationId);

        var count = await AsyncExecuter.CountAsync(ratingsOfDest);

        if (count == 0)
        {
            return new RatingStatsDto { AverageScore = 0, TotalCount = 0 };
        }

        // Calculamos promedio
        var average = query.Where(r => r.DestinationId == destinationId).Average(r => r.Score);

        return new RatingStatsDto
        {
            AverageScore = Math.Round(average, 1),
            TotalCount = count
        };
    }
    }
    // --- PUNTO 5.5: LISTAR COMENTARIOS CON NOMBRES DE USUARIO ---
    [AllowAnonymous]
    public async Task<List<RatingDto>> GetListByDestinationAsync(Guid destinationId)
    {
        // 5. DESACTIVAMOS EL FILTRO AQUÍ TAMBIÉN
        // Para que la lista traiga los comentarios de Juan, Pedro y María.
        using (_dataFilter.Disable<IUserOwned>())
        {
            // 1. Obtener Query
            var query = await Repository.GetQueryableAsync();

            // 2. Preparar filtro
            var queryFiltered = query
                .Where(r => r.DestinationId == destinationId)
                .OrderByDescending(r => r.CreationTime);

            // 3. Ejecutar
            var ratings = await AsyncExecuter.ToListAsync(queryFiltered);

            if (!ratings.Any()) return new List<RatingDto>();

            // 4. Obtener IDs de usuarios
            var userIds = ratings.Select(x => x.UserId).Distinct().ToList();

            // 5. Traer nombres de usuarios
            var users = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u.UserName);

            // 6. Mapear
            var resultDtos = new List<RatingDto>();
            foreach (var rating in ratings)
            {
                var dto = ObjectMapper.Map<Rating, RatingDto>(rating);

                if (userDict.ContainsKey(rating.UserId))
                {
                    dto.UserName = userDict[rating.UserId];
                }
                else
                {
                    dto.UserName = "Usuario (Cuenta eliminada)";
                }
                resultDtos.Add(dto);
            }

            return resultDtos;
        }
    }
}
