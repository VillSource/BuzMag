using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;

public sealed record GetJobLevelCategoriesQuery : IRequest<IReadOnlyList<JobLevelCategoryDto>>;
