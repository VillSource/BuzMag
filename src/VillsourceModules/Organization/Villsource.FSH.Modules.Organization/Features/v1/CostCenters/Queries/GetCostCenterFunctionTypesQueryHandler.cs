using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Queries;

public sealed class GetCostCenterFunctionTypesQueryHandler
    : IRequestHandler<GetCostCenterFunctionTypesQuery, IReadOnlyList<CostCenterFunctionTypeDto>>
{
    private static readonly IReadOnlyList<CostCenterFunctionTypeDto> CachedItems =
        CostCenterFunctionType.Items.Select(x => x.ToDto()).ToList();

    public ValueTask<IReadOnlyList<CostCenterFunctionTypeDto>> Handle(
        GetCostCenterFunctionTypesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ValueTask.FromResult(CachedItems);
    }
}
