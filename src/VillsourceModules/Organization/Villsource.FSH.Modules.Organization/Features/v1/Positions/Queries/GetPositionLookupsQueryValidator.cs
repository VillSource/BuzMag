using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionLookupsQueryValidator : AbstractValidator<GetPositionLookupsQuery>
{
    public GetPositionLookupsQueryValidator()
    {
        RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
        RuleFor(x => x.OrgUnitRefId).MaximumLength(VillsourceId.KeySizes)
            .When(x => !string.IsNullOrWhiteSpace(x.OrgUnitRefId));
    }
}