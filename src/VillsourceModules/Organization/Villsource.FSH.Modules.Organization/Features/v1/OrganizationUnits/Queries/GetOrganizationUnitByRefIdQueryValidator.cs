using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetOrganizationUnitByRefIdQueryValidator : AbstractValidator<GetOrganizationUnitByRefIdQuery>
{
    public GetOrganizationUnitByRefIdQueryValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
    }
}