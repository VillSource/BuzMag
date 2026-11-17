using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class SearchPositionDelegationsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchPositionDelegationsQuery, PagedResponse<PositionDelegationSummaryDto>>
{
    public async ValueTask<PagedResponse<PositionDelegationSummaryDto>> Handle(
        SearchPositionDelegationsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await dbContext.PositionDelegations
            .ApplySpecification(new SearchPositionDelegationsSpec(request))
            .ToPagedResponseAsync(request, cancellationToken);
    }
}