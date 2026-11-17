using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class GetPositionDelegationByRefIdQueryValidator : AbstractValidator<GetPositionDelegationByRefIdQuery>
{
    public GetPositionDelegationByRefIdQueryValidator() =>
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
}