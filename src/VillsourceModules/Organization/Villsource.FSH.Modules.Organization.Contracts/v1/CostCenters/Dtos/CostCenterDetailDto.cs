namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;

public sealed record CostCenterDetailDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityCode,
    string LegalEntityName,
    CostCenterNodeDto? ParentCostCenter,
    CostCenterTaxBranchDto? DefaultTaxBranch,
    string Code,
    string Name,
    string InternationalName,
    string? Description,
    CostCenterFunctionTypeDto FunctionType,
    Guid? ResponsibleUserId,
    Guid? OrgUnitId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    bool IsPostingBlocked,
    IReadOnlyList<CostCenterNodeDto> ChildCostCenters,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);