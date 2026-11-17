using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Queries;

public sealed class SearchJobFamiliesQueryValidator : AbstractValidator<SearchJobFamiliesQuery>
{
    public SearchJobFamiliesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).When(x => x.PageNumber.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).When(x => x.PageSize.HasValue);
    }
}
