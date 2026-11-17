using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class SearchOrganizationUnitsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchOrganizationUnitsQuery, PagedResponse<OrganizationUnitSummaryDto>>
{
    public async ValueTask<PagedResponse<OrganizationUnitSummaryDto>> Handle(
        SearchOrganizationUnitsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new SearchOrganizationUnitsSpec(request: request);
        return await dbContext.OrganizationUnits
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(pagination: request, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}