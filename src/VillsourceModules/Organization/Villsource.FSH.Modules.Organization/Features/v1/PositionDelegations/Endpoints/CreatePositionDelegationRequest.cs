namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

internal sealed record CreatePositionDelegationRequest(
    string LegalEntityRefId,
    string DelegatorRefId,
    string DelegateRefId,
    string Type,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocScope = null,
    string? Reason = null);
