using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class SearchOrganizationUnitsQueryValidator : AbstractValidator<SearchOrganizationUnitsQuery>
{
    public SearchOrganizationUnitsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).When(x => x.PageNumber.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
        RuleFor(x => x.LegalEntityRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.LegalEntityRefId));
        RuleFor(x => x.ParentRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ParentRefId));
        RuleFor(x => x.ManagerPositionRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ManagerPositionRefId));
        RuleFor(x => x.Type).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Type));
    }
}