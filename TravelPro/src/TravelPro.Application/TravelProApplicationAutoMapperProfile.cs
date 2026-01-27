using AutoMapper;
using System;
using TravelPro.Destinations;
using TravelPro.Experiences;
using TravelPro.Experiences.Dtos;
using TravelPro.Ratings;
using TravelPro.Watchlists;
using TravelPro.Watchlists.Dtos;
using TravelPro.Notifications;
using TravelPro.Notifications.Dtos;
namespace TravelPro;

public class TravelProApplicationAutoMapperProfile : Profile
{
    public TravelProApplicationAutoMapperProfile()
    {
        CreateMap<Destination, DestinationDto>();
        CreateMap<CreateUpdateDestinationDto, Destination>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifierId, opt => opt.Ignore())
            .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
            .ForMember(dest => dest.ExtraProperties, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());
        CreateMap<Rating, RatingDto>();
        CreateMap<CreateUpdateRatingDto, Rating>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifierId, opt => opt.Ignore())
            .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
            .ForMember(dest => dest.ExtraProperties, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.Destination, opt => opt.Ignore()) // evita ciclos
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<CreateUpdateExperienceDto, Experience>()
               .ConstructUsing(dto => new Experience(
                   Guid.NewGuid(),      // Generamos el ID aquí
                   dto.DestinationId,
                   dto.Title,
                   dto.Description,
                   dto.Sentiment
               ))
        .ForMember(dest => dest.Id, opt => opt.Ignore())             // Ya lo pusimos en el constructor
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())   // Auditoría
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())      // Auditoría
                .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore()) // Auditoría
                .ForMember(dest => dest.LastModifierId, opt => opt.Ignore()) // Auditoría
        .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())      // Soft Delete
                .ForMember(dest => dest.DeleterId, opt => opt.Ignore())      // Soft Delete
                .ForMember(dest => dest.DeletionTime, opt => opt.Ignore())   // Soft Delete
                .ForMember(dest => dest.UserId, opt => opt.Ignore());   // Asignado manualmente en AppService


        CreateMap<Experience, ExperienceDto>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore());

        CreateMap<Watchlist, WatchlistDto>()
    .ForMember(dest => dest.DestinationName, opt => opt.Ignore())    // Se llenan manual
    .ForMember(dest => dest.DestinationCountry, opt => opt.Ignore());

        CreateMap<Notification, NotificationDto>();
        // evita problemas con relaciones
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
