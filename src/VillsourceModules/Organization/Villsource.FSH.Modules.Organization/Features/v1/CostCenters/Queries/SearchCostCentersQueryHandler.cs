using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Queries;


public sealed class SearchCostCentersQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<SearchCostCentersQuery, PagedResponse<CostCenterSummaryDto>>
{
    public async ValueTask<PagedResponse<CostCenterSummaryDto>> Handle(
        SearchCostCentersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new SearchCostCentersSpec(request: request);

        return await dbContext.CostCenters
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(
                pagination: request,
                cancellationToken: cancellationToken);
    }
}

