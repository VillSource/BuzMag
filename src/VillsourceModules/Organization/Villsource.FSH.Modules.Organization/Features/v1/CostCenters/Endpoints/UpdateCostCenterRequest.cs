namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Endpoints;

internal sealed record UpdateCostCenterRequest(
    string Code,
    string Name,
    string IntlName,
    string FunctionType,
    string? Description = null,
    string? ParentRefId = null,
    string? TaxBranchRefId = null,
    string? ResponsibleRefId = null,
    string? OrgUnitRefId = null,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null);
