using Microsoft.Extensions.DependencyInjection;
using TravelPro.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AutoMapper;

namespace TravelPro;

[DependsOn(
    typeof(TravelProApplicationModule),
    typeof(TravelProDomainTestModule),
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule)
)]
public class TravelProApplicationTestModule : AbpModule
{
    
}
