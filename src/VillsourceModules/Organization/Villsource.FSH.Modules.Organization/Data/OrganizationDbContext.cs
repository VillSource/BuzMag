using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;
using Villsource.FSH.Modules.Organization.Domain.DocumentTypes;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.FSH.Modules.Organization.Domain.Positions;

namespace Villsource.FSH.Modules.Organization.Data;

public sealed class OrganizationDbContext(
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
    DbContextOptions<OrganizationDbContext> options,
    IOptions<DatabaseOptions> settings,
    IHostEnvironment environment)
    : BaseDbContext(multiTenantContextAccessor, options, settings, environment)
{
    public const string Schema = "organization";

    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<OrganizationUnitType> OrganizationUnitTypes => Set<OrganizationUnitType>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<TaxBranch> TaxBranches => Set<TaxBranch>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<JobGrade> JobGrades => Set<JobGrade>();
    public DbSet<JobFamily> JobFamilies => Set<JobFamily>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<PositionDelegation> PositionDelegations => Set<PositionDelegation>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();

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
