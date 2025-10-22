using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Users;
using System.Security.Claims;
using System.Collections.Generic;

namespace TravelPro.EntityFrameworkCore;
// 1. Definimos la misma clase "falsa" que usamos en las pruebas
public class DummyCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;
    public Guid? Id => null;
    public string UserName => null;
    public string Name => null;
    public string SurName => null;
    public string PhoneNumber => null;
    public bool PhoneNumberVerified => false;
    public string Email => null;
    public bool EmailVerified => false;
    public Guid? TenantId => null;
    public string[] Roles => Array.Empty<string>();
    public Claim FindClaim(string claimType) => null;
    public Claim[] FindClaims(string claimType) => Array.Empty<Claim>();
    public Claim[] GetAllClaims() => Array.Empty<Claim>();
    public bool IsInRole(string roleName) => false;
}
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
        
        return new TravelProDbContext(builder.Options, new DummyCurrentUser());
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TravelPro.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
