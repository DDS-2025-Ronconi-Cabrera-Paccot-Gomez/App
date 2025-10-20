using System;
using TravelPro.Destinations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TravelPro.Ratings;

public class Rating : AuditedAggregateRoot<Guid>
{
    public Guid DestinationId { get; private set; }
    public Destination Destination { get; private set; }
    public Guid UserId { get; private set; }
    public IdentityUser User { get; private set; }
    public int Score { get; private set; }
    public string? Comment { get; private set; }

    //Los atributos "createdAt", "updatedAt" y "deletedAt" se manejan automáticamente a través de la herencia de AuditedAggregateRoot

    private Rating() { }

    public Rating(Guid destinationId, Guid userId, int score, string? comment = null)
    {
        if (score < 1 || score > 5)
            throw new ArgumentException("El puntaje debe estar entre 1 y 5.");

        DestinationId = destinationId;
        UserId = userId;
        Score = score;
        Comment = comment;
    }
}


