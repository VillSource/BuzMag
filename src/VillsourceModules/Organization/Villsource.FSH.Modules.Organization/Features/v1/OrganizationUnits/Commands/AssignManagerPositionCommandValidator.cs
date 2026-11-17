using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class AssignManagerPositionCommandValidator : AbstractValidator<AssignManagerPositionCommand>
{
    public AssignManagerPositionCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.ManagerPositionRefId).MaximumLength(VillsourceId.KeySizes).When(x => !string.IsNullOrWhiteSpace(x.ManagerPositionRefId));
    }
}