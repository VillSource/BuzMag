using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class SearchOrganizationUnitsSpec : Specification<OrganizationUnit, OrganizationUnitSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<OrganizationUnit, object>>> SortMappings =
        new Dictionary<string, Expression<Func<OrganizationUnit, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = e => e.Code,
            ["name"] = e => e.Name,
            ["internationalName"] = e => e.InternationalName,
            ["type"] = e => e.Type,
            ["level"] = e => e.Level,
            ["createdOnUtc"] = e => e.CreatedOnUtc
        };

    public SearchOrganizationUnitsSpec(SearchOrganizationUnitsQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsActive.HasValue)
        {
            Where(e => e.IsActive == request.IsActive.Value);
        }

        if (request.RootOnly)
        {
            Where(e => e.ParentId == null);
        }
        else if (!string.IsNullOrWhiteSpace(request.ParentRefId))
        {
            var parentRefId = request.ParentRefId.Trim();
            Where(e => e.Parent != null && e.Parent.ReferenceId == parentRefId);
        }

        if (request.VacantManagerOnly)
        {
            Where(e => e.ManagerPositionId == null || e.ManagerPosition == null || e.ManagerPosition.IsDeleted || !e.ManagerPosition.IsActive);
        }
        else if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
        {
            var managerPosRefId = request.ManagerPositionRefId.Trim();
            Where(e => e.ManagerPosition != null && !e.ManagerPosition.IsDeleted && e.ManagerPosition.ReferenceId == managerPosRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var legalEntityRefId = request.LegalEntityRefId.Trim();
            Where(e => e.LegalEntity != null && e.LegalEntity.ReferenceId == legalEntityRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var type = request.Type.Trim().ToUpperInvariant();
            Where(e => e.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(e =>
                EF.Functions.ILike(e.Code, $"%{keyword}%") ||
                EF.Functions.ILike(e.Name, $"%{keyword}%") ||
                EF.Functions.ILike(e.InternationalName, $"%{keyword}%") ||
                (e.ManagerPosition != null && !e.ManagerPosition.IsDeleted && EF.Functions.ILike(e.ManagerPosition.Title, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () =>
            {
                OrderBy(e => e.Level);
                ThenBy(e => e.Code);
            },
            sortMappings: SortMappings);

        Select(e => new OrganizationUnitSummaryDto(
            e.ReferenceId,
            e.Code,
            e.Name,
            e.InternationalName,
            e.Type,
            e.LegalEntity != null ? e.LegalEntity.ReferenceId : null,
            e.Parent != null ? e.Parent.ReferenceId : null,
            e.Parent != null ? e.Parent.Name : null,
            e.Level,
            e.HierarchyPath,
            e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.ReferenceId : null,
            e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Code : null,
            e.ManagerPosition != null && !e.ManagerPosition.IsDeleted ? e.ManagerPosition.Title : null,
            e.IsActive,
            e.Children.Count(c => !c.IsDeleted)));
    }
}