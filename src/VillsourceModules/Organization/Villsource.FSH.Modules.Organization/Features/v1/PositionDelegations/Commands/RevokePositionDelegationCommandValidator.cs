using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class RevokePositionDelegationCommandValidator : AbstractValidator<RevokePositionDelegationCommand>
{
    public RevokePositionDelegationCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.RevocationReason).MaximumLength(PositionDelegationRules.MaxReasonLength);
    }
}