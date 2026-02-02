using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Destinations;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Applications.Destinations;

[Collection(TravelProTestConsts.CollectionDefinitionName)]
public class EfCoreDestinationAppService_Tests : DestinationAppService_Tests<TravelProEntityFrameworkCoreTestModule>
{

}
