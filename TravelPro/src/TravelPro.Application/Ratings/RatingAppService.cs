using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
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

    public RatingAppService(IRepository<Rating, Guid> repository, IObjectMapper objectMapper)
        : base(repository)
    {
        _objectMapper = objectMapper;
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

        await Repository.InsertAsync(rating);
        return _objectMapper.Map<Rating, RatingDto>(rating);
    }
}
