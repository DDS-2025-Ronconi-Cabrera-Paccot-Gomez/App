using System;
using TravelPro.Destinations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace TravelPro.Ratings;

public class RatingDto : AuditedEntityDto<Guid>
{
    public Guid DestinationId { get; private set; }
    public Guid UserId { get; private set; }
    public int Score { get; private set; }
    public string? Comment { get; private set; }
}
