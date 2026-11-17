using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Domain.Jobs;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class SearchJobsSpec : Specification<Job, JobSummaryDto>
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Job, object>>> SortMappings =
        new Dictionary<string, Expression<Func<Job, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = j => j.Code,
            ["name"] = j => j.Name,
            ["internationalName"] = j => j.InternationalName,
            ["jobFamily"] = j => j.JobFamily!.Name,
            ["rankOrder"] = j => j.DefaultJobGrade!.RankOrder,
            ["isActive"] = j => j.IsActive,
            ["createdOnUtc"] = j => j.CreatedOnUtc
        };

    public SearchJobsSpec(SearchJobsQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.JobFamilyRefId))
        {
            var familyRefId = request.JobFamilyRefId.Trim();
            Where(j => j.JobFamily != null && j.JobFamily.ReferenceId == familyRefId);
        }

        if (!string.IsNullOrWhiteSpace(request.DefaultJobGradeRefId))
        {
            var gradeRefId = request.DefaultJobGradeRefId.Trim();
            Where(j => j.DefaultJobGrade!.ReferenceId == gradeRefId);
        }

        if (request.IsActive.HasValue)
        {
            Where(j => j.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            Where(j =>
                EF.Functions.ILike(j.Code, $"%{keyword}%") ||
                EF.Functions.ILike(j.Name, $"%{keyword}%") ||
                EF.Functions.ILike(j.InternationalName, $"%{keyword}%") ||
                (j.JobFamily != null && EF.Functions.ILike(j.JobFamily.Name, $"%{keyword}%")));
        }

        ApplySortingOverride(
            sortExpression: request.Sort,
            applyDefaultOrdering: () => OrderBy(j => j.Code),
            sortMappings: SortMappings);

        Select(j => new JobSummaryDto(
            j.ReferenceId,
            j.Code,
            j.Name,
            j.InternationalName,
            j.JobFamily != null ? j.JobFamily.ReferenceId : null,
            j.JobFamily != null ? j.JobFamily.Code : null,
            j.JobFamily != null ? j.JobFamily.Name : null,
            j.DefaultJobGrade!.ReferenceId,
            j.DefaultJobGrade!.Code,
            j.DefaultJobGrade!.RankOrder,
            j.IsActive));
    }
}