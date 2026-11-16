using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

public sealed record GetCostCenterByRefIdQuery(string RefId) : IRequest<CostCenterDetailDto>;