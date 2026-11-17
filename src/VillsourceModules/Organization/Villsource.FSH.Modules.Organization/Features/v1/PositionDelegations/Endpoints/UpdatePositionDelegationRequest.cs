namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

internal sealed record UpdatePositionDelegationRequest(
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocumentTypeScope = null,
    string? Reason = null);