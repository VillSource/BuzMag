using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades;

// =============================================================================
// DTOs
// =============================================================================

/// <summary>
/// Lightweight projection for dropdown selectors and approval matrix configuration.
/// </summary>
public sealed record JobGradeLookupDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder);

/// <summary>
/// Paginated grid summary projection for job grades.
/// </summary>
public sealed record JobGradeSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description,
    bool IsActive);

/// <summary>
/// Full detail projection for a single job grade.
/// </summary>
public sealed record JobGradeDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

// =============================================================================
// Queries
// =============================================================================

public sealed record SearchJobGradesQuery : IPagedQuery, IRequest<PagedResponse<JobGradeSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public int? MinRankOrder { get; set; }
    public int? MaxRankOrder { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record GetJobGradeLookupsQuery(bool ActiveOnly = true)
    : IRequest<IReadOnlyList<JobGradeLookupDto>>;

public sealed record GetJobGradeByRefIdQuery(string RefId)
    : IRequest<JobGradeDetailDto>;

// =============================================================================
// Commands
// =============================================================================

public sealed record CreateJobGradeCommand(
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;

public sealed record UpdateJobGradeCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description = null) : IRequest<string>;

public sealed record ActivateJobGradeCommand(string RefId) : IRequest<string>;

public sealed record DeactivateJobGradeCommand(string RefId) : IRequest<string>;

public sealed record DeleteJobGradeCommand(string RefId) : IRequest<string>;