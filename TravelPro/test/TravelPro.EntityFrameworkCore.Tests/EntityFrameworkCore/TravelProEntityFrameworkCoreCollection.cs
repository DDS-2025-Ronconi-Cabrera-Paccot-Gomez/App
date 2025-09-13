using Xunit;

namespace TravelPro.EntityFrameworkCore;

[CollectionDefinition(TravelProTestConsts.CollectionDefinitionName)]
public class TravelProEntityFrameworkCoreCollection : ICollectionFixture<TravelProEntityFrameworkCoreFixture>
{

}
