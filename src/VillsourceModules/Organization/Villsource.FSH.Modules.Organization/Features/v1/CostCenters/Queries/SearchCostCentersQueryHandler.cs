using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
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

        Guid? orgUnitId = null;
        if (!string.IsNullOrWhiteSpace(request.OrgUnitRefId))
        {
            string orgUnitRefId = request.OrgUnitRefId.Trim();
            orgUnitId = await dbContext.OrganizationUnits
                .AsNoTracking()
                .Where(ou => ou.ReferenceId == orgUnitRefId)
                .Select(ou => (Guid?)ou.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!orgUnitId.HasValue)
            {
                int pageNumber = request.PageNumber is null or <= 0 ? 1 : request.PageNumber.Value;
                int pageSize = request.PageSize is null or <= 0 ? 20 : Math.Min(request.PageSize.Value, 100);

                return new PagedResponse<CostCenterSummaryDto>
                {
                    Items = Array.Empty<CostCenterSummaryDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }
        }

        var spec = new SearchCostCentersSpec(request: request, orgUnitId: orgUnitId);

        PagedResponse<CostCenterSearchRow> pagedRows = await dbContext.CostCenters
            .ApplySpecification(specification: spec)
            .ToPagedResponseAsync(
                pagination: request,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        List<Guid> orgUnitIds = pagedRows.Items
            .Where(row => row.OrgUnitId.HasValue)
            .Select(row => row.OrgUnitId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> orgUnitRefIds = orgUnitIds.Count > 0
            ? await dbContext.OrganizationUnits
                .AsNoTracking()
                .Where(ou => orgUnitIds.Contains(ou.Id))
                .ToDictionaryAsync(ou => ou.Id, ou => ou.ReferenceId, cancellationToken)
                .ConfigureAwait(false)
            : new Dictionary<Guid, string>();

        List<CostCenterSummaryDto> items = pagedRows.Items
            .Select(row => row.OrgUnitId.HasValue && orgUnitRefIds.TryGetValue(row.OrgUnitId.Value, out string? refId)
                ? row.Dto with { OrgUnitRefId = refId }
                : row.Dto)
            .ToList();

        return new PagedResponse<CostCenterSummaryDto>
        {
            Items = items,
            PageNumber = pagedRows.PageNumber,
            PageSize = pagedRows.PageSize,
            TotalCount = pagedRows.TotalCount,
            TotalPages = pagedRows.TotalPages
        };
    }
}

