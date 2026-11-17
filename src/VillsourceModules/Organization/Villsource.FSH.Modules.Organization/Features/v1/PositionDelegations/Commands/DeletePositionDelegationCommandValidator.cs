using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class DeletePositionDelegationCommandValidator : AbstractValidator<DeletePositionDelegationCommand>
{
    public DeletePositionDelegationCommandValidator() =>
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
}