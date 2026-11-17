using System.Linq.Expressions;
using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;
using Villsource.FSH.Modules.Organization.Domain.CostCenters;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Specifications;

internal sealed record CostCenterSearchRow(Guid? OrgUnitId, CostCenterSummaryDto Dto);

internal sealed class SearchCostCentersSpec : Specification<CostCenter, CostCenterSearchRow>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<CostCenter, object>>> SortMappings =
        new Dictionary<string, Expression<Func<CostCenter, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = c => c.Code,
            ["name"] = c => c.Name,
            ["internationalName"] = c => c.InternationalName,
            ["functionType"] = c => c.FunctionType,
            ["isActive"] = c => c.IsActive,
            ["isPostingBlocked"] = c => c.IsPostingBlocked,
            ["validFrom"] = c => c.ValidFrom!,
            ["validTo"] = c => c.ValidTo!,
            ["legalEntityName"] = c => c.LegalEntity!.LegalName,
            ["createdOnUtc"] = c => c.CreatedOnUtc
        };

    public SearchCostCentersSpec(SearchCostCentersQuery request, Guid? orgUnitId = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var legalEntityRefId = request.LegalEntityRefId.Trim();
            Where(c => c.LegalEntity!.ReferenceId == legalEntityRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.ParentCostCenterRefId))
        {
            var parentRefId = request.ParentCostCenterRefId.Trim();
            Where(c => c.ParentCostCenter != null && c.ParentCostCenter.ReferenceId == parentRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.FunctionType))
        {
            var functionType = CostCenterFunctionType.FromKey(request.FunctionType);
            Where(c => c.FunctionType == functionType);
        }

        if (!string.IsNullOrWhiteSpace(request.ResponsiblePositionRefId))
        {
            var posRefId = request.ResponsiblePositionRefId.Trim();
            Where(c => c.ResponsiblePosition != null && !c.ResponsiblePosition.IsDeleted && c.ResponsiblePosition.ReferenceId == posRefId);
        }

        if (orgUnitId.HasValue)
        {
            Where(c => c.OrgUnitId == orgUnitId.Value);
        }

        if (request.IsActive.HasValue)
        {
            Where(c => c.IsActive == request.IsActive.Value);
        }

        if (request.IsPostingBlocked.HasValue)
        {
            Where(c => c.IsPostingBlocked == request.IsPostingBlocked.Value);
        }

        if (request.EffectiveAtUtc.HasValue)
        {
            var utcTarget = request.EffectiveAtUtc.Value.ToUniversalTime();
            Where(c =>
                (c.ValidFrom == null || c.ValidFrom <= utcTarget) &&
                (c.ValidTo == null || c.ValidTo >= utcTarget));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(c =>
                EF.Functions.ILike(c.Code, $"%{keyword}%") ||
                EF.Functions.ILike(c.Name, $"%{keyword}%") ||
                EF.Functions.ILike(c.InternationalName, $"%{keyword}%") ||
                (c.Description != null && EF.Functions.ILike(c.Description, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(c => c.Code),
            sortMappings: SortMappings);

        Select(c => new CostCenterSearchRow(
            c.OrgUnitId,
            new CostCenterSummaryDto(
                c.ReferenceId,
                c.LegalEntity!.ReferenceId,
                c.LegalEntity!.LegalName,
                c.ParentCostCenter != null ? c.ParentCostCenter.ReferenceId : null,
                c.ParentCostCenter != null ? c.ParentCostCenter.Code : null,
                c.Code,
                c.Name,
                c.InternationalName,
                (string)c.FunctionType,
                c.DefaultTaxBranch != null ? c.DefaultTaxBranch.BranchCode : null,
                c.ResponsiblePosition != null && !c.ResponsiblePosition.IsDeleted ? c.ResponsiblePosition.ReferenceId : null,
                c.ResponsiblePosition != null && !c.ResponsiblePosition.IsDeleted ? c.ResponsiblePosition.Code : null,
                c.ResponsiblePosition != null && !c.ResponsiblePosition.IsDeleted ? c.ResponsiblePosition.Title : null,
                null,
                c.ValidFrom,
                c.ValidTo,
                c.IsActive,
                c.IsPostingBlocked,
                c.Children.Count(child => !child.IsDeleted && child.IsActive))));
    }
}

