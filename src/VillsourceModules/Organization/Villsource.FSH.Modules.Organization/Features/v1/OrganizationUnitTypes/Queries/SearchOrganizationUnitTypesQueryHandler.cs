using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Queries;

public sealed class SearchOrganizationUnitTypesQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchOrganizationUnitTypesQuery, PagedResponse<OrganizationUnitTypeSummaryDto>>
{
    public async ValueTask<PagedResponse<OrganizationUnitTypeSummaryDto>> Handle(
        SearchOrganizationUnitTypesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await dbContext.OrganizationUnitTypes
            .ApplySpecification(new SearchOrganizationUnitTypesSpec(request))
            .ToPagedResponseAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }
}
