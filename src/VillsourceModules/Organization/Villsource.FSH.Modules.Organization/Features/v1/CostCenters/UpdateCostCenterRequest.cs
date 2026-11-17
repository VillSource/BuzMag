namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters;
internal sealed record UpdateCostCenterRequest(
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    string? Description = null,
    string? ParentCostCenterRefId = null,
    string? DefaultTaxBranchRefId = null,
    Guid? ResponsibleUserId = null,
    Guid? OrgUnitId = null,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null);
