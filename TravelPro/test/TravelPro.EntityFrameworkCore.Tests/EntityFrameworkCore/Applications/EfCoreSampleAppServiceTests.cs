using TravelPro.Samples;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Applications;

[Collection(TravelProTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<TravelProEntityFrameworkCoreTestModule>
{

}
