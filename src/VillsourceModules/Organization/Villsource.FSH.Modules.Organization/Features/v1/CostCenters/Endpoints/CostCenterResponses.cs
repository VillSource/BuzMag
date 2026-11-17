namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Endpoints;

internal sealed record CostCenterSummaryResponse(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    string? ParentRefId,
    string? ParentCode,
    string Code,
    string Name,
    string IntlName,
    string FunctionType,
    string? TaxBranchCode,
    string? ResponsibleRefId,
    string? ResponsibleCode,
    string? ResponsibleTitle,
    string? OrgUnitRefId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    bool IsPostingBlocked,
    int ActiveChildCount);

internal sealed record CostCenterDetailResponse(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityCode,
    string LegalEntityName,
    CostCenterNodeResponse? Parent,
    CostCenterTaxBranchResponse? TaxBranch,
    string Code,
    string Name,
    string IntlName,
    string? Description,
    CostCenterFunctionTypeResponse FunctionType,
    string? ResponsibleRefId,
    string? ResponsibleCode,
    string? ResponsibleTitle,
    string? OrgUnitRefId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    bool IsPostingBlocked,
    IReadOnlyList<CostCenterNodeResponse> Children,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record CostCenterNodeResponse(
    string RefId,
    string Code,
    string Name,
    string IntlName,
    string FunctionType,
    bool IsActive,
    bool IsPostingBlocked);

internal sealed record CostCenterTaxBranchResponse(
    string RefId,
    string BranchCode,
    string Name,
    bool IsHeadOffice);

internal sealed record CostCenterFunctionTypeResponse(
    string Key,
    string DisplayName,
    string FinancialStatementLine,
    bool IsDirectOperatingCost,
    bool RequiresPeriodEndAllocation);
