namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

internal sealed record PositionDelegationSummaryResponse(
    string RefId,
    string LegalEntityRefId,
    string DelegatorRefId,
    string DelegatorCode,
    string DelegatorTitle,
    string DelegateRefId,
    string DelegateCode,
    string DelegateTitle,
    string Type,
    string? DocScope,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsRevoked);

internal sealed record PositionDelegationDetailResponse(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    DelegationPositionResponse Delegator,
    DelegationPositionResponse Delegate,
    DelegationTypeResponse Type,
    string? DocScope,
    string? Reason,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsRevoked,
    DateTimeOffset? RevokedOnUtc,
    string? RevokedBy,
    string? RevocationReason,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record EffectiveDelegationResponse(
    string RefId,
    string Type,
    string? DocScope,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    DelegationPositionResponse Delegate);

internal sealed record DelegationPositionResponse(
    string RefId,
    string Code,
    string Title,
    string JobGradeCode,
    int JobGradeRankOrder);

internal sealed record DelegationTypeResponse(
    string Key,
    string DisplayName,
    bool RequiresDocScope);
