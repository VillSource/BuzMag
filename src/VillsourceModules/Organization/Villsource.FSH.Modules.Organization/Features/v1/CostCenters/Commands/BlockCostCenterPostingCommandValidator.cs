using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class BlockCostCenterPostingCommandValidator : AbstractValidator<BlockCostCenterPostingCommand>
{
    public BlockCostCenterPostingCommandValidator() => RuleFor(x => x.RefId).ValidRefId();
}

