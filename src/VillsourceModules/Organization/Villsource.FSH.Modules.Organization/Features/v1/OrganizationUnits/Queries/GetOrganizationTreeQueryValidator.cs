using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Queries;

public sealed class GetOrganizationTreeQueryValidator : AbstractValidator<GetOrganizationTreeQuery>
{
    public GetOrganizationTreeQueryValidator()
    {
        RuleFor(x => x.LegalEntityRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.MaxDepth).GreaterThanOrEqualTo(0).When(x => x.MaxDepth.HasValue);
    }
}
