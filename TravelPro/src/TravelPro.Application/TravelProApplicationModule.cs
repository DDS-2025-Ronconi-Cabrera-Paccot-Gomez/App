using Microsoft.Extensions.DependencyInjection;
using TravelPro.TravelProGeo;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using TravelPro.TravelProGeo;
using TravelPro.Destinations;
using TravelPro.GeoServices;
namespace TravelPro;


[DependsOn(
    typeof(TravelProDomainModule),
    typeof(TravelProApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
     typeof(AbpAutoMapperModule)
    )]
public class TravelProApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<TravelProApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<TravelProApplicationModule>(validate: true);
        });
        context.Services.AddTransient<ICitySearchAPIService,  CitySearchAPIService>();
        context.Services.AddTransient<ICitySearchService, CitySearchService>();
    }
}
