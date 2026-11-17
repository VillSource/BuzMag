using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Specifications;

public sealed class SearchOrganizationUnitTypesSpec : Specification<OrganizationUnitType, OrganizationUnitTypeSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<OrganizationUnitType, object>>> SortMappings =
        new Dictionary<string, Expression<Func<OrganizationUnitType, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = t => t.Code,
            ["name"] = t => t.Name,
            ["internationalName"] = t => t.InternationalName,
            ["hierarchyLevel"] = t => t.HierarchyLevel,
            ["isActive"] = t => t.IsActive,
            ["createdOnUtc"] = t => t.CreatedOnUtc
        };

    public SearchOrganizationUnitTypesSpec(SearchOrganizationUnitTypesQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsActive.HasValue)
        {
            Where(t => t.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(t =>
                EF.Functions.ILike(t.Code, $"%{keyword}%") ||
                EF.Functions.ILike(t.Name, $"%{keyword}%") ||
                EF.Functions.ILike(t.InternationalName, $"%{keyword}%"));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(t => t.HierarchyLevel),
            sortMappings: SortMappings);

        Select(t => new OrganizationUnitTypeSummaryDto(
            t.ReferenceId,
            t.Code,
            t.Name,
            t.InternationalName,
            t.HierarchyLevel,
            t.Description,
            t.IsActive));
    }
}
