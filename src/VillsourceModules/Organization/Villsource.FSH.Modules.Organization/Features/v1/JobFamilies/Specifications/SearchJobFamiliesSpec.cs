using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;
using Villsource.FSH.Modules.Organization.Domain.JobFamilies;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Specifications;

public sealed class SearchJobFamiliesSpec : Specification<JobFamily, JobFamilySummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<JobFamily, object>>> SortMappings =
        new Dictionary<string, Expression<Func<JobFamily, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = f => f.Code,
            ["name"] = f => f.Name,
            ["internationalName"] = f => f.InternationalName,
            ["isActive"] = f => f.IsActive,
            ["createdOnUtc"] = f => f.CreatedOnUtc
        };

    public SearchJobFamiliesSpec(SearchJobFamiliesQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsActive.HasValue)
        {
            Where(f => f.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(f =>
                EF.Functions.ILike(f.Code, $"%{keyword}%") ||
                EF.Functions.ILike(f.Name, $"%{keyword}%") ||
                EF.Functions.ILike(f.InternationalName, $"%{keyword}%"));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(f => f.Code),
            sortMappings: SortMappings);

        Select(f => new JobFamilySummaryDto(
            f.ReferenceId,
            f.Code,
            f.Name,
            f.InternationalName,
            f.Description,
            f.IsActive));
    }
}
