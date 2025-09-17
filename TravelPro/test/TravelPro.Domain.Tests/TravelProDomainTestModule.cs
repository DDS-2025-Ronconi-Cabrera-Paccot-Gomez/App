using Volo.Abp.Modularity;

namespace TravelPro;

[DependsOn(
    typeof(TravelProDomainModule),
    typeof(TravelProTestBaseModule)
)]
public class TravelProDomainTestModule : AbpModule
{

}
