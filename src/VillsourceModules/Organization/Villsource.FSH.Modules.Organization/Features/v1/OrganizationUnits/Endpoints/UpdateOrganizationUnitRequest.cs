namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

internal sealed record UpdateOrganizationUnitRequest(
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? Description = null,
    string? CostCenterCode = null);