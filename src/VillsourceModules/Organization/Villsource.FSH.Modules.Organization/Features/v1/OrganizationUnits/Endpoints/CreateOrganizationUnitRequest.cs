namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

internal sealed record CreateOrganizationUnitRequest(
    string Code,
    string Name,
    string IntlName,
    string TypeRefId,
    string? Description = null,
    string? LegalEntityRefId = null,
    string? ParentRefId = null,
    string? CostCenterRefId = null,
    string? ManagerRefId = null,
    bool AllowExternalManager = false);
