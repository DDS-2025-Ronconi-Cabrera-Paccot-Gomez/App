using TravelPro.Samples;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Domains;

[Collection(TravelProTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<TravelProEntityFrameworkCoreTestModule>
{

}
