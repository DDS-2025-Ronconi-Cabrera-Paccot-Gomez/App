using Volo.Abp.Modularity;

namespace TravelPro;

/* Inherit from this class for your domain layer tests. */
public abstract class TravelProDomainTestBase<TStartupModule> : TravelProTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
