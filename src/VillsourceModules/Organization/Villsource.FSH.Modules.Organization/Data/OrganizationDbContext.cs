using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Data;

public sealed class OrganizationDbContext(
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
    DbContextOptions<OrganizationDbContext> options,
    IOptions<DatabaseOptions> settings,
    IHostEnvironment environment)
    : BaseDbContext(multiTenantContextAccessor, options, settings, environment)
{
    public const string Schema = "organization";

    public DbSet<Domain.Organization> Organizations => Set<Domain.Organization>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<Position> Positions => Set<Position>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply (ApplyTenantIsolationByDefault)
        // sees fully-configured entities, including child types reached via HasMany navigation.
        base.OnModelCreating(modelBuilder);
    }
}
