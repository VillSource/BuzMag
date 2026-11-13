using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;

namespace Villsource.FSH.Modules.Organization.Services;

public sealed class OrganizationUnitService(OrganizationDbContext dbContext) : IOrganizationUnitService
{
    public async Task<List<OrganizationUnit>> GetDescendantsAsync(OrganizationUnit ancestor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ancestor);
        var descendants = dbContext.OrganizationUnits
            .Where(ou => ou.Path.StartsWith(ancestor.Path))
            .OrderBy(ou => ou.Path)
            .AsTracking();

        return await descendants.ToListAsync(ct).ConfigureAwait(false);
    }
}