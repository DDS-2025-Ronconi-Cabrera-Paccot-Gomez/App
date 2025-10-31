using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
namespace TravelPro.Ratings;

[Authorize]
public class RatingAppService :
    CrudAppService<
        Rating, //The Rating entity
        RatingDto, //Used to show ratings
        Guid, //Primary key of the rating entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateRatingDto>, //Used to create/update a book
    IRatingAppService //implement the IBookAppService
{
    public RatingAppService(IRepository<Rating, Guid> repository)
        : base(repository)
    {

    }

    public override async Task<RatingDto> CreateAsync(CreateUpdateRatingDto input)
    {
        // Mapeamos el DTO (que trae Score, Comment, DestinationId) a la entidad
        var rating = ObjectMapper.Map<CreateUpdateRatingDto, Rating>(input);

        // ---- PASO 3.2: Obtener el usuario autenticado ----
        // 'CurrentUser' es una propiedad de la clase base 'ApplicationService'.
        // Usamos .Value para obtener el Guid, ya que 'Id' es un Guid? (nullable).
        // Si el usuario no está logueado, esto lanzará una excepción,
        // lo cual es correcto (no se puede calificar sin loguearse).
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


        // 3. Guarda en el repositorio
        await Repository.InsertAsync(rating);

        // 4. Devuelve el DTO
        return ObjectMapper.Map<Rating, RatingDto>(rating);
    }
}