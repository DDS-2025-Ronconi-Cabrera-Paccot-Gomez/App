using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TravelPro.Data;

/* This is used if database provider does't define
 * ITravelProDbSchemaMigrator implementation.
 */
public class NullTravelProDbSchemaMigrator : ITravelProDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
