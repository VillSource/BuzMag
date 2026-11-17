using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Commands;
using Villsource.FSH.Modules.Organization.Domain.PositionDelegations;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Commands;

public sealed class UpdatePositionDelegationCommandValidator : AbstractValidator<UpdatePositionDelegationCommand>
{
    public UpdatePositionDelegationCommandValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.DelegationType)
            .NotEmpty()
            .Must(DelegationType.IsValidKey)
            .WithMessage($"DelegationType must be one of: {DelegationType.AllowedKeysDescription}.");

        RuleFor(x => x.DocumentTypeScope).MaximumLength(PositionDelegationRules.MaxDocumentTypeScopeLength);
        RuleFor(x => x.Reason).MaximumLength(PositionDelegationRules.MaxReasonLength);
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
    }
}