namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

internal sealed record UpdatePositionDelegationRequest(
    string Type,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string? DocScope = null,
    string? Reason = null);
