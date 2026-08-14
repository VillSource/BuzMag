using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Services;

public sealed class OrganizationUnitService(OrganizationDbContext dbContext) : IOrganizationUnitService
{
    public async Task<List<OrganizationUnit>> GetDescendantsAsync(OrganizationUnit ancestor, CancellationToken ct = default)
    {
        var descendants = dbContext.OrganizationUnits
            .Where(ou => ou.OrganizationId == ancestor.OrganizationId)
            .Where(ou => ou.Path.StartsWith(ancestor.Path))
            .OrderBy(ou => ou.Path);

        return await descendants.ToListAsync(ct).ConfigureAwait(false);
    }
}