namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

internal sealed record UpdateOrganizationUnitRequest(
    string Code,
    string Name,
    string IntlName,
    string TypeRefId,
    string? Description = null,
    string? CostCenterRefId = null);
