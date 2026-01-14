using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TravelPro.EntityFrameworkCore.Tests.Fakes;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
namespace TravelPro.EntityFrameworkCore;

[DependsOn(
    typeof(TravelProApplicationTestModule),
    typeof(TravelProEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(TravelProDomainModule),
    typeof(Volo.Abp.AutoMapper.AbpAutoMapperModule),
    typeof(TravelProApplicationModule)
)]
public class TravelProEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });


        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });

        // Deshabilitamos OpenIddict (evita NullReference)
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false; // Desactiva workers para las pruebas
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

        context.Services.Replace(
        ServiceDescriptor.Singleton<ICurrentUser, FakeCurrentUser>()
                             );
        context.Services.AddSingleton<FakeCurrentUser>(sp =>
        (FakeCurrentUser)sp.GetRequiredService<ICurrentUser>());
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<TravelProApplicationModule>(validate: true);
        });
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        try
        {
            // Intentamos cerrar la conexión limpiamente
            _sqliteConnection?.Dispose();
        }
        catch
        {
            // Si falla (porque ya se cerró sola), no nos importa. 
            // Lo importante es que la prueba lógica haya pasado.
        }
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TravelProDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new TravelProDbContext(options, new DummyCurrentUser()))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
