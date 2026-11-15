using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters;

#region DTOs

public sealed record CostCenterFunctionTypeDto(
    string Key,
    string DisplayName,
    string FinancialStatementLine,
    bool IsDirectOperatingCost,
    bool RequiresPeriodEndAllocation);

public sealed record CostCenterNodeDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string FunctionType,
    bool IsActive,
    bool IsPostingBlocked);

public sealed record CostCenterTaxBranchDto(
    string RefId,
    string BranchCode,
    string Name,
    bool IsHeadOffice);

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
    Guid? ResponsibleUserId,
    Guid? OrgUnitId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    bool IsPostingBlocked,
    int ActiveChildCount);

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

#endregion

#region Queries

public sealed record SearchCostCentersQuery : IPagedQuery, IRequest<PagedResponse<CostCenterSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentCostCenterRefId { get; set; }
    public string? FunctionType { get; set; }
    public Guid? ResponsibleUserId { get; set; }
    public Guid? OrgUnitId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPostingBlocked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}

public sealed record GetCostCenterByRefIdQuery(string RefId) : IRequest<CostCenterDetailDto>;

public sealed record GetCostCenterFunctionTypesQuery : IRequest<IReadOnlyList<CostCenterFunctionTypeDto>>;

#endregion

#region Commands

public sealed record CreateCostCenterCommand(
    string LegalEntityRefId,
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
    DateTimeOffset? ValidTo = null,
    bool IsActive = true,
    bool IsPostingBlocked = false) : IRequest<string>;

public sealed record UpdateCostCenterCommand(
    string RefId,
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
    DateTimeOffset? ValidTo = null) : IRequest<string>;

public sealed record ActivateCostCenterCommand(string RefId) : IRequest<string>;

public sealed record DeactivateCostCenterCommand(string RefId) : IRequest<string>;

public sealed record BlockCostCenterPostingCommand(string RefId) : IRequest<string>;

public sealed record UnblockCostCenterPostingCommand(string RefId) : IRequest<string>;

public sealed record DeleteCostCenterCommand(string RefId) : IRequest<string>;

#endregion