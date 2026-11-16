namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

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