using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

/// <summary>
/// Resolves all subordinate positions beneath a manager position in breadth-first order.
/// Used for manager document visibility and span-of-control queries.
/// </summary>
public sealed record GetPositionSubtreeQuery(
    string RefId,
    int MaxDepth = 10,
    bool IncludeInactive = false) : IRequest<IReadOnlyList<PositionNodeDto>>;