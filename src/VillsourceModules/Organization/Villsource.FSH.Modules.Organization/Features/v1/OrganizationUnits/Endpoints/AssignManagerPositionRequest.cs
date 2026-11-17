namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

internal sealed record AssignManagerPositionRequest(
    string? ManagerRefId,
    bool AllowExternalManager = false);
