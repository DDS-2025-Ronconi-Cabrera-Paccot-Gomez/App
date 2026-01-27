using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata; //  NECESARIO para IMutableEntityType
using System; //  NECESARIO para Func<T> y Guid
using System.Linq.Expressions; //  NECESARIO para Expression
using TravelPro.Destinations;
using TravelPro.Experiences;
using TravelPro.Metrics;
using TravelPro.Notifications;
using TravelPro.Ratings;
using TravelPro.Watchlists;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Users; //  NECESARIO para ICurrentUser


namespace TravelPro.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class TravelProDbContext :
    AbpDbContext<TravelProDbContext>,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Experience> Experiences { get; set; }

    public DbSet<Watchlist> Watchlists { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<ApiMetric> ApiMetrics { get; set; }
    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion
    // 1. CAMPO PARA GUARDAR EL USUARIO (DE PASOS ANTERIORES)
    private readonly ICurrentUser _currentUser;
    // 2. CONSTRUCTOR MODIFICADO (DE PASOS ANTERIORES)
    public TravelProDbContext(
        DbContextOptions<TravelProDbContext> options,
        ICurrentUser currentUser) // <-- Inyectamos ICurrentUser
        : base(options)
    {
        _currentUser = currentUser; // <-- Lo guardamos
    }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        //Mapeo Destination
        builder.Entity<Destination>(b =>
        {
            b.ToTable(TravelProConsts.DbTablePrefix + "Destinations",
                TravelProConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.OwnsOne(x => x.Coordinates, coord =>
            {
                coord.Property(c => c.Latitude)
                     .IsRequired()
                     .HasColumnName("Coordinates_Latitude");

                coord.Property(c => c.Longitude)
                     .IsRequired()
                     .HasColumnName("Coordinates_Longitude");
            });
        });

        //Mapeo Rating
        builder.Entity<Rating>(b =>
        {
            b.ToTable(TravelProConsts.DbTablePrefix + "Ratings",
                TravelProConsts.DbSchema);
            b.ConfigureByConvention(); 

        
             b.Property(x => x.Score)
             .IsRequired();

        
            b.Property(x => x.Comment)
             .HasMaxLength(500);

            // Relaciones para genererar FK
            b.HasOne(r => r.Destination)
             .WithMany() 
             .HasForeignKey(r => r.DestinationId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(r => r.User)
             .WithMany() 
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            //índice compuesto para evitar duplicados
            b.HasIndex(x => new { x.DestinationId, x.UserId })
             .IsUnique();


});
        builder.Entity<IdentityUser>(b =>
        {
            b.Property<string>("ProfilePhoto")
                .IsRequired(false);
        });

        //Mapeo experiencia
        builder.Entity<Experience>(b =>
        {
            b.ToTable(TravelProConsts.DbTablePrefix + "Experiences", TravelProConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Tags).HasMaxLength(256);

            // Índice para buscar rápido por Tags (Punto 4.6)
            b.HasIndex(x => x.Tags);
        });
        //Mapeo lista de favoritos
        builder.Entity<Watchlist>(b =>
        {
            b.ToTable(TravelProConsts.DbTablePrefix + "Watchlists", TravelProConsts.DbSchema);
            b.ConfigureByConvention();

            // Índice Único: Un usuario solo puede tener 1 entrada por destino
            b.HasIndex(x => new { x.UserId, x.DestinationId }).IsUnique();
        });

        //Mapeo metricas
        builder.Entity<ApiMetric>(b =>
        {
            b.ToTable(TravelProConsts.DbTablePrefix + "ApiMetrics", TravelProConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.ApiName); // Índice para búsquedas rápidas
        });
    }

    
    // 3. AQUÍ VAN LOS NUEVOS MÉTODOS DEL FILTRO (DEL TUTORIAL) 
    //    Van al final de la clase, al mismo nivel que el constructor y OnModelCreating.

    protected bool IsUserOwnedFilterEnabled => DataFilter?.IsEnabled<IUserOwned>() ?? false;

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IUserOwned).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder);

        if (typeof(IUserOwned).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> userOwnedFilter =
                e => !IsUserOwnedFilterEnabled ||
                     !_currentUser.IsAuthenticated ||
                     EF.Property<Guid>(e, "UserId") == _currentUser.Id;

            expression = expression == null
                ? userOwnedFilter
                : QueryFilterExpressionHelper.CombineExpressions(expression, userOwnedFilter);
        }

        return expression;
    }
}
    //builder.Entity<YourEntity>(b =>
    //{
    //    b.ToTable(TravelProConsts.DbTablePrefix + "YourEntities", TravelProConsts.DbSchema);
    //    b.ConfigureByConvention(); //auto configure for the base class props
    //    //...
    //});


