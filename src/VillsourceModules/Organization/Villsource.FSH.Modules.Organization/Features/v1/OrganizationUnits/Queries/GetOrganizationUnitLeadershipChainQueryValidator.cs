using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetOrganizationUnitLeadershipChainQueryValidator : AbstractValidator<GetOrganizationUnitLeadershipChainQuery>
{
    public GetOrganizationUnitLeadershipChainQueryValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}