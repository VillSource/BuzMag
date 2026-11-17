namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;

public sealed record CostCenterSummaryDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    string? ParentCostCenterRefId,
    string? ParentCostCenterCode,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    string? DefaultTaxBranchCode,
    string? ResponsiblePositionRefId,
    string? ResponsiblePositionCode,
    string? ResponsiblePositionTitle,
    string? OrgUnitRefId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    bool IsPostingBlocked,
    int ActiveChildCount);