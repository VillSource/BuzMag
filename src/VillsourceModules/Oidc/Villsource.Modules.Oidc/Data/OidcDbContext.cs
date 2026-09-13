using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Villsource.Modules.Oidc.Domain;

namespace Villsource.Modules.Oidc.Data;

public sealed class OidcDbContext(
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
    DbContextOptions<OidcDbContext> options,
    IOptions<DatabaseOptions> settings,
    IHostEnvironment environment)
    : BaseDbContext(multiTenantContextAccessor, options, settings, environment)
{
    public const string Schema = "Oidc";

    public DbSet<Ping> Pings => Set<Ping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OidcDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply (ApplyTenantIsolationByDefault)
        // sees fully-configured entities, including child types reached via HasMany navigation.
        base.OnModelCreating(modelBuilder);
    }
}