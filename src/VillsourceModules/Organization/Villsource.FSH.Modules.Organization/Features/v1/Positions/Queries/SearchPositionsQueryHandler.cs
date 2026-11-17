using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Positions.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class SearchPositionsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchPositionsQuery, PagedResponse<PositionSummaryDto>>
{
    public async ValueTask<PagedResponse<PositionSummaryDto>> Handle(
        SearchPositionsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await dbContext.Positions
            .ApplySpecification(new SearchPositionsSpec(request))
            .ToPagedResponseAsync(request, cancellationToken);
    }
}