using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

public sealed record GetPositionByRefIdQuery(string RefId) : IRequest<PositionDetailDto>;