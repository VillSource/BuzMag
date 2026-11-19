using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class SearchLegalEntitiesQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<SearchLegalEntitiesQuery, PagedResponse<LegalEntitySummaryDto>>
{
    public async ValueTask<PagedResponse<LegalEntitySummaryDto>> Handle(
        SearchLegalEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new SearchLegalEntitiesSpec(request: request);

        return await dbContext.LegalEntities
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(
                pagination: request,
                cancellationToken: cancellationToken);
    }
}