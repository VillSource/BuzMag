using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionReportingChainQueryValidator : AbstractValidator<GetPositionReportingChainQuery>
{
    public GetPositionReportingChainQueryValidator()
    {
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(VillsourceId.KeySizes);
        RuleFor(x => x.MaxDepth).InclusiveBetween(1, 50);
    }
}