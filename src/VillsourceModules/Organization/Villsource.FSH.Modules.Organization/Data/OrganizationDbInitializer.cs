using FSH.Framework.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Data;

public sealed partial class OrganizationDbInitializer(
    OrganizationDbContext dbContext,
    ICurrentUserService currentUserService,
    ILogger<OrganizationDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            LogMigrationsApplied();
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var tenant = ResolveTenantIdentifier();
        LogSeedingStarted(tenant);

        if (!await dbContext.JobFamilies.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var jobFamilies = CreateJobFamilies();
            await dbContext.JobFamilies.AddRangeAsync(jobFamilies, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedJobFamiliesSuccess(jobFamilies.Count);
        }

        if (!await dbContext.JobGrades.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var jobGrades = CreateJobGrades();
            await dbContext.JobGrades.AddRangeAsync(jobGrades, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedJobGradesSuccess(jobGrades.Count);
        }

        OrganizationUnitType? defaultOuType = null;
        if (!await dbContext.OrganizationUnitTypes.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var ouTypes = CreateOrganizationUnitTypes();
            defaultOuType = ouTypes[0];
            await dbContext.OrganizationUnitTypes.AddRangeAsync(ouTypes, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogSeedOuTypesSuccess(ouTypes.Count);
        }

        if (!await dbContext.OrganizationUnits.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            defaultOuType ??= await dbContext.OrganizationUnitTypes
                .OrderBy(t => t.HierarchyLevel)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (defaultOuType is not null)
            {
                var rootUnit = CreateDefaultOrganizationUnit(tenant, defaultOuType.Id);
                await dbContext.OrganizationUnits.AddAsync(rootUnit, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                LogSeedOrganizationUnitsSuccess(1);
            }
        }
    }

    private string ResolveTenantIdentifier()
    {
        var tenant = currentUserService.GetTenant();
        if (!string.IsNullOrWhiteSpace(tenant))
        {
            return tenant.Trim();
        }

        return dbContext.TenantInfo?.Identifier
            ?? dbContext.TenantInfo?.Id
            ?? "DEFAULT";
    }

    private static List<JobFamily> CreateJobFamilies() =>
    [
        JobFamily.Create(
            "GEN",
            "General",
            "General",
            "Default job family placeholder. Modify or add job families as needed.")
    ];

    private static List<JobGrade> CreateJobGrades() =>
    [
        JobGrade.Create(
            "G1",
            "Grade 1",
            "Grade 1",
            1,
            "Default job grade placeholder. Modify or add job grades as needed.",
            JobLevelCategory.Operational)
    ];

    private static List<OrganizationUnitType> CreateOrganizationUnitTypes() =>
    [
        OrganizationUnitType.Create(
            "ORG",
            "Organization",
            "Organization",
            0,
            "Default organization unit type placeholder. Modify or add unit types as needed.")
    ];

    private static OrganizationUnit CreateDefaultOrganizationUnit(string tenant, Guid organizationUnitTypeId)
    {
        var code = tenant.Length > 50 ? tenant[..50] : tenant;

        return OrganizationUnit.Create(
            code: code,
            name: tenant,
            internationalName: tenant,
            organizationUnitTypeId: organizationUnitTypeId,
            description: "Default organization unit placeholder. Modify or add units as needed.");
    }

    [LoggerMessage(LogLevel.Information, "[Organization] applied migrations")]
    partial void LogMigrationsApplied();

    [LoggerMessage(LogLevel.Information, "[Organization] seeding default placeholder organization for tenant {Tenant}")]
    partial void LogSeedingStarted(string tenant);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {JobFamiliesCount} JobFamilies successfully")]
    partial void LogSeedJobFamiliesSuccess(int jobFamiliesCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {JobGradesCount} JobGrades successfully")]
    partial void LogSeedJobGradesSuccess(int jobGradesCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {OrganizationTypeCount} OrganizationUnitTypes successfully")]
    partial void LogSeedOuTypesSuccess(int organizationTypeCount);

    [LoggerMessage(LogLevel.Information, "[Organization] seeded {OrganizationUnitCount} OrganizationUnits successfully")]
    partial void LogSeedOrganizationUnitsSuccess(int organizationUnitCount);
}
