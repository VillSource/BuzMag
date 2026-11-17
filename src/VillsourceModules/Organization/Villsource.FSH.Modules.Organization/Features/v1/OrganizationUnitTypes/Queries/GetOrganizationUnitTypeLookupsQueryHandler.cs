using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Queries;

public sealed class GetOrganizationUnitTypeLookupsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetOrganizationUnitTypeLookupsQuery, IReadOnlyList<OrganizationUnitTypeLookupDto>>
{
    public async ValueTask<IReadOnlyList<OrganizationUnitTypeLookupDto>> Handle(
        GetOrganizationUnitTypeLookupsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = dbContext.OrganizationUnitTypes.AsNoTracking();
        if (request.ActiveOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.HierarchyLevel)
            .ThenBy(t => t.Code)
            .Select(t => new OrganizationUnitTypeLookupDto(
                t.ReferenceId,
                t.Code,
                t.Name,
                t.InternationalName,
                t.HierarchyLevel))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
