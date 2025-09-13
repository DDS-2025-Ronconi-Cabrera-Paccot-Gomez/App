using Volo.Abp.Modularity;

namespace TravelPro;

public abstract class TravelProApplicationTestBase<TStartupModule> : TravelProTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
