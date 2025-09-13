using TravelPro.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TravelPro.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TravelProEntityFrameworkCoreModule),
    typeof(TravelProApplicationContractsModule)
)]
public class TravelProDbMigratorModule : AbpModule
{
}
