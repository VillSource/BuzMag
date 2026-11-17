using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries;

public sealed class GetJobLevelCategoriesQueryHandler
    : IRequestHandler<GetJobLevelCategoriesQuery, IReadOnlyList<JobLevelCategoryDto>>
{
    public ValueTask<IReadOnlyList<JobLevelCategoryDto>> Handle(
        GetJobLevelCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<JobLevelCategoryDto> items = JobLevelCategory.Items
            .OrderBy(c => c.SortOrder)
            .Select(c => new JobLevelCategoryDto(c.Key, c.DisplayName, c.SortOrder))
            .ToList();

        return ValueTask.FromResult(items);
    }
}
