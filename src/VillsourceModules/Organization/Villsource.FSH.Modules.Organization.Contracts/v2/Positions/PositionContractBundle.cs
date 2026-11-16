using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v2.Positions;

// =============================================================================
// DTOs
// =============================================================================

/// <summary>
/// Lightweight projection for dropdown selectors (reporting manager, org unit head, delegation).
/// </summary>
public sealed record PositionLookupDto(
    string RefId,
    string LegalEntityRefId,
    Guid OrgUnitId,
    string Code,
    string Title,
    string InternationalTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsPrimaryLeadership);

public sealed record PositionNodeDto(
    string RefId,
    string Code,
    string Title,
    string InternationalTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsPrimaryLeadership,
    bool IsActive);

public sealed record PositionJobDto(
    string RefId,
    string Code,
    string Name);

public sealed record PositionJobGradeDto(
    string RefId,
    string Code,
    string Name,
    int RankOrder);

public sealed record PositionCostCenterDto(
    string RefId,
    string Code,
    string Name);

public sealed record PositionSummaryDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityName,
    Guid OrgUnitId,
    string JobCode,
    string JobGradeCode,
    int JobGradeRankOrder,
    string? CostCenterCode,
    string? ReportsToPositionRefId,
    string? ReportsToPositionCode,
    string Code,
    string Title,
    string InternationalTitle,
    int HeadcountCapacity,
    bool IsPrimaryLeadership,
    bool IsActive,
    int ActiveDirectReportCount);

public sealed record PositionDetailDto(
    string RefId,
    string LegalEntityRefId,
    string LegalEntityCode,
    string LegalEntityName,
    Guid OrgUnitId,
    PositionJobDto Job,
    PositionJobGradeDto JobGrade,
    PositionCostCenterDto? CostCenter,
    PositionNodeDto? ReportsToPosition,
    PositionNodeDto? DottedLinePosition,
    string Code,
    string Title,
    string InternationalTitle,
    int HeadcountCapacity,
    bool IsPrimaryLeadership,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    IReadOnlyList<PositionNodeDto> DirectReports,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

// =============================================================================
// Queries
// =============================================================================

public sealed record SearchPositionsQuery : IPagedQuery, IRequest<PagedResponse<PositionSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public Guid? OrgUnitId { get; set; }
    public string? JobRefId { get; set; }
    public string? JobGradeRefId { get; set; }
    public string? CostCenterRefId { get; set; }
    public string? ReportsToPositionRefId { get; set; }
    public bool? IsPrimaryLeadership { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record GetPositionLookupsQuery(
    string? LegalEntityRefId = null,
    Guid? OrgUnitId = null,
    bool ActiveOnly = true,
    bool LeadershipOnly = false) : IRequest<IReadOnlyList<PositionLookupDto>>;

public sealed record GetPositionByRefIdQuery(string RefId) : IRequest<PositionDetailDto>;

/// <summary>
/// Resolves the upward chain of command from a starting position up to the top of the reporting tree.
/// Used directly by Document Approval workflow resolvers.
/// </summary>
public sealed record GetPositionReportingChainQuery(
    string RefId,
    int MaxDepth = 15,
    bool IncludeInactive = false) : IRequest<IReadOnlyList<PositionNodeDto>>;

/// <summary>
/// Resolves all subordinate positions beneath a manager position in breadth-first order.
/// Used for manager document visibility and span-of-control queries.
/// </summary>
public sealed record GetPositionSubtreeQuery(
    string RefId,
    int MaxDepth = 10,
    bool IncludeInactive = false) : IRequest<IReadOnlyList<PositionNodeDto>>;

// =============================================================================
// Commands
// =============================================================================

public sealed record CreatePositionCommand(
    string LegalEntityRefId,
    Guid OrgUnitId,
    string JobRefId,
    string Code,
    string Title,
    string InternationalTitle,
    string? JobGradeRefId = null,
    string? CostCenterRefId = null,
    string? ReportsToPositionRefId = null,
    string? DottedLinePositionRefId = null,
    int HeadcountCapacity = 1,
    bool IsPrimaryLeadership = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null,
    bool IsActive = true) : IRequest<string>;

public sealed record UpdatePositionCommand(
    string RefId,
    Guid OrgUnitId,
    string JobRefId,
    string JobGradeRefId,
    string Code,
    string Title,
    string InternationalTitle,
    string? CostCenterRefId = null,
    string? ReportsToPositionRefId = null,
    string? DottedLinePositionRefId = null,
    int HeadcountCapacity = 1,
    bool IsPrimaryLeadership = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null) : IRequest<string>;

public sealed record ActivatePositionCommand(string RefId) : IRequest<string>;

public sealed record DeactivatePositionCommand(string RefId) : IRequest<string>;

public sealed record DeletePositionCommand(string RefId) : IRequest<string>;