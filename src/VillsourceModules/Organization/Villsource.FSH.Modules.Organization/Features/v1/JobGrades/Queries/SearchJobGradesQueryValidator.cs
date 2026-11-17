using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries
{
    public sealed class SearchJobGradesQueryValidator : AbstractValidator<SearchJobGradesQuery>
    {
        public SearchJobGradesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
            RuleFor(x => x.MinRankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder).When(x => x.MinRankOrder.HasValue);
            RuleFor(x => x.MaxRankOrder).GreaterThanOrEqualTo(JobGradeRules.MinRankOrder).When(x => x.MaxRankOrder.HasValue);
            RuleFor(x => x.MaxRankOrder)
                .GreaterThanOrEqualTo(x => x.MinRankOrder!.Value)
                .When(x => x.MinRankOrder.HasValue && x.MaxRankOrder.HasValue);
        }
    }

}