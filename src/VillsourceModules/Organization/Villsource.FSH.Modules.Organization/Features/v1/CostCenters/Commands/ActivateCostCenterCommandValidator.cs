using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class ActivateCostCenterCommandValidator : AbstractValidator<ActivateCostCenterCommand>
{
    public ActivateCostCenterCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}
