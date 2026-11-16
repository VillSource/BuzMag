using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

/// <summary>
/// Resolves the upward chain of command from a starting position up to the top of the reporting tree.
/// Used directly by Document Approval workflow resolvers.
/// </summary>
public sealed record GetPositionReportingChainQuery(
    string RefId,
    int MaxDepth = 15,
    bool IncludeInactive = false) : IRequest<IReadOnlyList<PositionNodeDto>>;