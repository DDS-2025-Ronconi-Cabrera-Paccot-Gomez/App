using System.Threading.Tasks;

namespace TravelPro.Data;

public interface ITravelProDbSchemaMigrator
{
    Task MigrateAsync();
}
