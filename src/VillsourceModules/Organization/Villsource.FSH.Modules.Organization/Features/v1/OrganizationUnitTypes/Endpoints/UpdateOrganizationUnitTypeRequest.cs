namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

internal sealed record UpdateOrganizationUnitTypeRequest(
    string Code,
    string Name,
    string IntlName,
    int Level,
    string? Description = null);
