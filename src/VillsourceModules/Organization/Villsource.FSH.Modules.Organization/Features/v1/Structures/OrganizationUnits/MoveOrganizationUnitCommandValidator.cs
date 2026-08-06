using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandValidator : AbstractValidator<MoveOrganizationUnitCommand>
{
    public MoveOrganizationUnitCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.OrganizationId.HasValue != x.ParentId.HasValue)
            .WithMessage("Exactly one of 'NewParentId' or 'NewOrganizationId' must be provided.");
    }
}

public sealed class MoveOrganizationUnitBodyValidator : AbstractValidator<OrganizationUnitEndpoint.MoveOrganizationUnitBody>
{
    public MoveOrganizationUnitBodyValidator()
    {
        RuleFor(x => x)
            .Must(x => x.NewOrganizationId.HasValue != x.NewParentId.HasValue)
            .WithMessage("[BODY] Exactly one of 'NewParentId' or 'NewOrganizationId' must be provided.");
    }
}
