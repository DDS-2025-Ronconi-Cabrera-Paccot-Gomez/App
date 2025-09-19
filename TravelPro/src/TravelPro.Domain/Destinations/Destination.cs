using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Values;


namespace TravelPro.Destinations;

public class Destination : AuditedAggregateRoot <Guid>
{
    public string Name { get; set; }
    public string Country { get; set; }

    public int Population { get; set; }
    public string Photo { get; set; }
    public string Region { get; set; }
    public DateTime LastUpdated { get; set; }
    public Coordinate Coordinates { get; set; }

}

public class Coordinate : ValueObject
{
    public string Latitude { get; set; }
    public string Longitude { get; set; }

    //Constructor 
    public Coordinate(string latitude, string longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    // Sobrescribe el método GetAtomicValues para indicar las propiedades de valor.
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }
}

//-----------------------------------------------------------------------------------------------------------------------//

public class TravelProDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Destination, Guid> _DestinationRepository;

    public TravelProDataSeederContributor(IRepository<Destination, Guid> DestinationRepository)
    {
        _DestinationRepository = DestinationRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _DestinationRepository.GetCountAsync() <= 0)
        {
            await _DestinationRepository.InsertAsync(
                new Destination
                {
                    Name = "Concepcion del uruguay",
                    Country = "Argentina",
                    Population = 67464,
                    Region = "Litoral",
                    LastUpdated = DateTime.Now,
                    Photo = "",
                    Coordinates = new Coordinate(
                        latitude: "-32.48463",
                        longitude: "-58.23217"
                    )
                },
                autoSave: true
            );

            
        }
    }
}