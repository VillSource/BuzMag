using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class SearchLegalEntitiesQueryValidator : AbstractValidator<SearchLegalEntitiesQuery>
{
    public SearchLegalEntitiesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.CountryCode)
            .Length(2)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));
    }
}