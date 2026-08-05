using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class DeleteOrganizationUnitCommandValidator : AbstractValidator<DeleteOrganizationUnitCommand>
{
    public DeleteOrganizationUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().WithMessage("Id is required.");
    }
}