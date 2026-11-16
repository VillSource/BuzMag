using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;

public sealed record GetDelegationTypesQuery
    : IRequest<IReadOnlyList<DelegationTypeDto>>;