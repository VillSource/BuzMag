using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class ResolveEffectiveDelegationsQueryValidator : AbstractValidator<ResolveEffectiveDelegationsQuery>
{
    public ResolveEffectiveDelegationsQueryValidator()
    {
        RuleFor(x => x.DelegatorPositionRefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.DocumentTypeCode).MaximumLength(100);
    }
}