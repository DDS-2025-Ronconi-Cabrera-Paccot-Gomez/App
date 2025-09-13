using Volo.Abp.Modularity;

namespace TravelPro;

[DependsOn(
    typeof(TravelProApplicationModule),
    typeof(TravelProDomainTestModule)
)]
public class TravelProApplicationTestModule : AbpModule
{

}
