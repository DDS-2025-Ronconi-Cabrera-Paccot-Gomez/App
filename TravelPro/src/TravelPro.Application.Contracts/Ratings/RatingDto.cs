using System;
using TravelPro.Destinations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace TravelPro.Ratings;

public class RatingDto : AuditedEntityDto<Guid>
{
    public Guid DestinationId { get;  set; }
    public Guid UserId { get;  set; }
    public string UserName { get; set; }
    public int Score { get;  set; }
    public string? Comment { get;  set; }
}
