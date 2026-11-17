using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class SearchJobsQueryValidator : AbstractValidator<SearchJobsQuery>
{
    public SearchJobsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
        RuleFor(x => x.JobFamily).MaximumLength(JobRules.MaxJobFamilyLength);
        RuleFor(x => x.DefaultJobGradeRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultJobGradeRefId));
    }
}