using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Specifications;

public sealed class SearchPositionsSpec : Specification<Position, PositionSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Position, object>>> SortMappings =
        new Dictionary<string, Expression<Func<Position, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = p => p.Code,
            ["title"] = p => p.Title,
            ["internationalTitle"] = p => p.InternationalTitle,
            ["rankOrder"] = p => p.JobGrade!.RankOrder,
            ["isPrimaryLeadership"] = p => p.IsPrimaryLeadership,
            ["isActive"] = p => p.IsActive,
            ["createdOnUtc"] = p => p.CreatedOnUtc
        };

    public SearchPositionsSpec(SearchPositionsQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.LegalEntityRefId))
        {
            var leRef = request.LegalEntityRefId.Trim();
            Where(p => p.LegalEntity!.ReferenceId == leRef);
        }

        if (!string.IsNullOrWhiteSpace(request.OrgUnitRefId))
        {
            string ouRef = request.OrgUnitRefId.Trim();
            Where(p => p.OrgUnit != null && p.OrgUnit.ReferenceId == ouRef);
        }

        if (!string.IsNullOrWhiteSpace(request.JobRefId))
        {
            var jobRef = request.JobRefId.Trim();
            Where(p => p.Job!.ReferenceId == jobRef);
        }

        if (!string.IsNullOrWhiteSpace(request.JobGradeRefId))
        {
            var gradeRef = request.JobGradeRefId.Trim();
            Where(p => p.JobGrade!.ReferenceId == gradeRef);
        }

        if (!string.IsNullOrWhiteSpace(request.CostCenterRefId))
        {
            var ccRef = request.CostCenterRefId.Trim();
            Where(p => p.CostCenter != null && p.CostCenter.ReferenceId == ccRef);
        }

        if (!string.IsNullOrWhiteSpace(request.ReportsToPositionRefId))
        {
            var managerRef = request.ReportsToPositionRefId.Trim();
            Where(p => p.ReportsToPosition != null && p.ReportsToPosition.ReferenceId == managerRef);
        }

        if (request.IsPrimaryLeadership.HasValue)
        {
            Where(p => p.IsPrimaryLeadership == request.IsPrimaryLeadership.Value);
        }

        if (request.IsActive.HasValue)
        {
            Where(p => p.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(p =>
                EF.Functions.ILike(p.Code, $"%{keyword}%") ||
                EF.Functions.ILike(p.Title, $"%{keyword}%") ||
                EF.Functions.ILike(p.InternationalTitle, $"%{keyword}%"));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderByDescending(p => p.JobGrade!.RankOrder),
            sortMappings: SortMappings);

        Select(p => new PositionSummaryDto(
            p.ReferenceId,
            p.LegalEntity!.ReferenceId,
            p.LegalEntity!.LegalName,
            p.OrgUnit != null ? p.OrgUnit.ReferenceId : null,
            p.Job!.Code,
            p.JobGrade!.Code,
            p.JobGrade!.RankOrder,
            p.CostCenter != null ? p.CostCenter.Code : null,
            p.ReportsToPosition != null ? p.ReportsToPosition.ReferenceId : null,
            p.ReportsToPosition != null ? p.ReportsToPosition.Code : null,
            p.Code,
            p.Title,
            p.InternationalTitle,
            p.HeadcountCapacity,
            p.IsPrimaryLeadership,
            p.IsActive,
            p.DirectReports.Count(r => !r.IsDeleted && r.IsActive)));
    }
}