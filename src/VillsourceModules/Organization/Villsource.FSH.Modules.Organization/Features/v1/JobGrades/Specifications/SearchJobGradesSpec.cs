using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Specifications
{

    public sealed class SearchJobGradesSpec : Specification<JobGrade, JobGradeSummaryDto>
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<JobGrade, object>>> SortMappings =
            new Dictionary<string, Expression<Func<JobGrade, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = g => g.Code,
                ["name"] = g => g.Name,
                ["internationalName"] = g => g.InternationalName,
                ["rankOrder"] = g => g.RankOrder,
                ["isActive"] = g => g.IsActive,
                ["createdOnUtc"] = g => g.CreatedOnUtc
            };

        public SearchJobGradesSpec(SearchJobGradesQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.IsActive.HasValue)
            {
                Where(g => g.IsActive == request.IsActive.Value);
            }

            if (request.MinRankOrder.HasValue)
            {
                Where(g => g.RankOrder >= request.MinRankOrder.Value);
            }

            if (request.MaxRankOrder.HasValue)
            {
                Where(g => g.RankOrder <= request.MaxRankOrder.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                Where(g =>
                    EF.Functions.ILike(g.Code, $"%{keyword}%") ||
                    EF.Functions.ILike(g.Name, $"%{keyword}%") ||
                    EF.Functions.ILike(g.InternationalName, $"%{keyword}%"));
            }

            ApplySortingOverride(
                sortExpression: request.Sort,
                applyDefaultOrdering: () => OrderBy(g => g.RankOrder),
                sortMappings: SortMappings);

            Select(g => new JobGradeSummaryDto(
                g.ReferenceId,
                g.Code,
                g.Name,
                g.InternationalName,
                g.RankOrder,
                g.Description,
                g.IsActive));
        }
    }

}