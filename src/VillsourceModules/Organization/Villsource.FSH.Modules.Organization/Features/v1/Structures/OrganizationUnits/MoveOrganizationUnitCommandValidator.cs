using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandValidator : AbstractValidator<MoveOrganizationUnitCommand>
{
    public MoveOrganizationUnitCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.OrganizationId)  != !string.IsNullOrWhiteSpace(x.ParentId))
            .WithMessage("Exactly one of 'NewParentId' or 'NewOrganizationId' must be provided.");
    }
}
