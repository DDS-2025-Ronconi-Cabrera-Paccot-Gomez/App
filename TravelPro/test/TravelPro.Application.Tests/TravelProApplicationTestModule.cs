using Microsoft.Extensions.DependencyInjection;
using TravelPro.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TravelPro;

[DependsOn(
    typeof(TravelProApplicationModule),
    typeof(TravelProDomainTestModule),
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule)
)]
public class TravelProApplicationTestModule : AbpModule
{
    
}
