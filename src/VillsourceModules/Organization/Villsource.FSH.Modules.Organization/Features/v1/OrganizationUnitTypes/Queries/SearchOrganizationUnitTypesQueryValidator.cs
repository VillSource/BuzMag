using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Queries;

public sealed class SearchOrganizationUnitTypesQueryValidator : AbstractValidator<SearchOrganizationUnitTypesQuery>
{
    public SearchOrganizationUnitTypesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).When(x => x.PageNumber.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).When(x => x.PageSize.HasValue);
    }
}
