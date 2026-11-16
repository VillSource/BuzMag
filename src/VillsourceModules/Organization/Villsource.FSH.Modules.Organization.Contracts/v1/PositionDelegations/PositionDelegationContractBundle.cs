using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations;

// =============================================================================
// DTOs
// =============================================================================

public sealed record DelegationTypeDto(
    string Key,
    string DisplayName,
    bool RequiresDocumentScope);

public sealed record DelegationPositionDto(
    string RefId,
    string Code,
    string Title,
    string JobGradeCode,
    int JobGradeRankOrder);

/// <summary>
/// Active delegation projection resolved for a target position and optional document type at runtime.
/// Consumed directly by the Document Approval workflow engine.
/// </summary>
public sealed record EffectiveDelegationDto(
    string RefId,
    string DelegationType,
    string? DocumentTypeScope,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    DelegationPositionDto DelegatePosition);

public sealed record PositionDelegationSummaryDto(
    string RefId,
    string LegalEntityRefId,
    string DelegatorPositionRefId,
    string DelegatorPositionCode,
    string DelegatorPositionTitle,
    string DelegatePositionRefId,
    string DelegatePositionCode,
    string DelegatePositionTitle,
    string DelegationType,
    string? DocumentTypeScope,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsRevoked);

public sealed record PositionDelegationDetailDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    DelegationPositionDto DelegatorPosition,
    DelegationPositionDto DelegatePosition,
    DelegationTypeDto DelegationType,
    string? DocumentTypeScope,
    string? Reason,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsRevoked,
    DateTimeOffset? RevokedOnUtc,
    string? RevokedBy,
    string? RevocationReason,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

// =============================================================================
// Queries
// =============================================================================

public sealed record SearchPositionDelegationsQuery : IPagedQuery, IRequest<PagedResponse<PositionDelegationSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? DelegatorPositionRefId { get; set; }
    public string? DelegatePositionRefId { get; set; }
    public bool? IsRevoked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}

public sealed record GetPositionDelegationByRefIdQuery(string RefId)
    : IRequest<PositionDelegationDetailDto>;

public sealed record GetDelegationTypesQuery
    : IRequest<IReadOnlyList<DelegationTypeDto>>;

/// <summary>
/// Resolves all currently active acting/approval delegations for a given delegator position
/// and optional document type code at the specified timestamp (defaults to UtcNow).
/// </summary>
public sealed record ResolveEffectiveDelegationsQuery(
    string DelegatorPositionRefId,
    string? DocumentTypeCode = null,
    DateTimeOffset? AtUtc = null) : IRequest<IReadOnlyList<EffectiveDelegationDto>>;

// =============================================================================
// Commands
// =============================================================================

public sealed record CreatePositionDelegationCommand(
    string LegalEntityRefId,
    string DelegatorPositionRefId,
    string DelegatePositionRefId,
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocumentTypeScope = null,
    string? Reason = null) : IRequest<string>;

public sealed record UpdatePositionDelegationCommand(
    string RefId,
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocumentTypeScope = null,
    string? Reason = null) : IRequest<string>;

public sealed record RevokePositionDelegationCommand(
    string RefId,
    string? RevocationReason = null) : IRequest<string>;

public sealed record DeletePositionDelegationCommand(string RefId) : IRequest<string>;