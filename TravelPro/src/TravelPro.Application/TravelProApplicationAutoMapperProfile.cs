using AutoMapper;
using TravelPro.Destinations;

namespace TravelPro;

public class TravelProApplicationAutoMapperProfile : Profile
{
    public TravelProApplicationAutoMapperProfile()
    {
        CreateMap<Destination, DestinationDto>();
        CreateMap<CreateUpdateDestinationDto, Destination>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
