using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class DeleteOrganizationUnitTypeCommandValidator : AbstractValidator<DeleteOrganizationUnitTypeCommand>
{
    public DeleteOrganizationUnitTypeCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty();
    }
}
