using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using Volo.Abp.Validation;
using Shouldly;
using TravelPro;

namespace TravelPro.Destinations;
public abstract class DestinationAppService_Tests<TStartupModule> : TravelProApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDestinationAppService _DestinationAppService;

    protected DestinationAppService_Tests()
    {
        _DestinationAppService = GetRequiredService<IDestinationAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Destination()
    {
        //Act
        var result = await _DestinationAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "Concepcion del uruguay");
    }
    [Fact]
    public async Task Should_Create_A_Valid_Destination()
    {
        //Act
        var result = await _DestinationAppService.CreateAsync(
            new CreateUpdateDestinationDto
            {
                Name = "Ciudad De Buenos Aires",
                Country = "Argentina",
                Population = 3121707,
                Region = "Pampeana",
                LastUpdated = DateTime.Now,
                Photo = "https://bit.ly/47TQ5Ws",
                Coordinates = new Coordinate(
                        latitude: "-34.593",
                        longitude: "-58.224"
                    )
            }
        );

        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Ciudad De Buenos Aires");
    }
    [Fact]
    public async Task Should_Not_Create_A_Destination_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _DestinationAppService.CreateAsync(
                new CreateUpdateDestinationDto
                {
                    Name = "",
                    Country = "Argentina",
                    Population = 3121707,
                    Region = "Pampeana",
                    LastUpdated = DateTime.Now,
                    Photo = "https://bit.ly/47TQ5Ws",
                    Coordinates = new Coordinate(
                        latitude: "﻿-34.599722222222",
                        longitude: "-58.381944444444"
                    )
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }
}
