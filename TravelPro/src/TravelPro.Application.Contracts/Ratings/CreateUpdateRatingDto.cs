using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TravelPro.Ratings;

public class CreateUpdateRatingDto
{
    [Required]
    public Guid DestinationId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}
