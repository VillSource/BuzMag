namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

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