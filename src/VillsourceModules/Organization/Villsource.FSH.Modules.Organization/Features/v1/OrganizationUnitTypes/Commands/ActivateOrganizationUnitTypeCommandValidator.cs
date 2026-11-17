using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class ActivateOrganizationUnitTypeCommandValidator : AbstractValidator<ActivateOrganizationUnitTypeCommand>
{
    public ActivateOrganizationUnitTypeCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();
    }
}
