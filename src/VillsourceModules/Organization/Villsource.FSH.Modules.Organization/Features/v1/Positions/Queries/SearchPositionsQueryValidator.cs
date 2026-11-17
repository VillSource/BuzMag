using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class SearchPositionsQueryValidator : AbstractValidator<SearchPositionsQuery>
{
    public SearchPositionsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
        RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
        RuleFor(x => x.OrgUnitRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.OrgUnitRefId));
        RuleFor(x => x.JobRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.JobRefId));
        RuleFor(x => x.JobGradeRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.JobGradeRefId));
        RuleFor(x => x.CostCenterRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.CostCenterRefId));
        RuleFor(x => x.ReportsToPositionRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.ReportsToPositionRefId));
    }
}