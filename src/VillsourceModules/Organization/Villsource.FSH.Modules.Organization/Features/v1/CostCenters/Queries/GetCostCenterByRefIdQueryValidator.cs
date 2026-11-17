using FluentValidation;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Queries;

public sealed class GetCostCenterByRefIdQueryValidator : AbstractValidator<GetCostCenterByRefIdQuery>
{
    public GetCostCenterByRefIdQueryValidator() =>
        RuleFor(x => x.RefId).ValidRefId();
}

