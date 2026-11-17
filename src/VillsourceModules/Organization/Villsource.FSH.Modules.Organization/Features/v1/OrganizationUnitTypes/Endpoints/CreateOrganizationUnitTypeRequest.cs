namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

internal sealed record CreateOrganizationUnitTypeRequest(
    string Code,
    string Name,
    string IntlName,
    int Level,
    string? Description = null,
    bool IsActive = true);
