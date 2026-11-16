using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

public sealed record GetCostCenterFunctionTypesQuery : IRequest<IReadOnlyList<CostCenterFunctionTypeDto>>;