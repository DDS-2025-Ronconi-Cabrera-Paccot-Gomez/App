using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TravelPro.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class TravelProDbContextFactory : IDesignTimeDbContextFactory<TravelProDbContext>
{
    public TravelProDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        TravelProEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<TravelProDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new TravelProDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TravelPro.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
