using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;

/// <summary>
/// Resolves all currently active acting/approval delegations for a given delegator position
/// and optional document type code at the specified timestamp (defaults to UtcNow).
/// </summary>
public sealed record ResolveEffectiveDelegationsQuery(
    string DelegatorPositionRefId,
    string? DocumentTypeCode = null,
    DateTimeOffset? AtUtc = null) : IRequest<IReadOnlyList<EffectiveDelegationDto>>;