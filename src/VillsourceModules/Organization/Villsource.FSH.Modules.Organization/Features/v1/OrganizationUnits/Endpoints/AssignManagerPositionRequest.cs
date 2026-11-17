namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

internal sealed record AssignManagerPositionRequest(
    string? ManagerPositionRefId,
    bool AllowExternalManagerPosition = false);