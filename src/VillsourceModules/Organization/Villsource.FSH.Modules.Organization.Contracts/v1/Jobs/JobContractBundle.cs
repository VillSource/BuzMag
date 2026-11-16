using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs;

// =============================================================================
// DTOs
// =============================================================================

/// <summary>
/// Lightweight projection for dropdown selectors when creating or editing positions.
/// </summary>
public sealed record JobLookupDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? JobFamily,
    string DefaultJobGradeRefId,
    string DefaultJobGradeCode,
    int DefaultJobGradeRankOrder);

public sealed record JobGradeRefDto(
    string RefId,
    string Code,
    string Name,
    int RankOrder);

public sealed record JobSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? JobFamily,
    string DefaultJobGradeRefId,
    string DefaultJobGradeCode,
    int DefaultJobGradeRankOrder,
    bool IsActive);

public sealed record JobDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? JobFamily,
    string? Description,
    JobGradeRefDto DefaultJobGrade,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

// =============================================================================
// Queries
// =============================================================================

public sealed record SearchJobsQuery : IPagedQuery, IRequest<PagedResponse<JobSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? JobFamily { get; set; }
    public string? DefaultJobGradeRefId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record GetJobLookupsQuery(
    bool ActiveOnly = true,
    string? JobFamily = null) : IRequest<IReadOnlyList<JobLookupDto>>;

public sealed record GetJobByRefIdQuery(string RefId) : IRequest<JobDetailDto>;

// =============================================================================
// Commands
// =============================================================================

public sealed record CreateJobCommand(
    string Code,
    string Name,
    string InternationalName,
    string DefaultJobGradeRefId,
    string? JobFamily = null,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;

public sealed record UpdateJobCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string DefaultJobGradeRefId,
    string? JobFamily = null,
    string? Description = null) : IRequest<string>;

public sealed record ActivateJobCommand(string RefId) : IRequest<string>;

public sealed record DeactivateJobCommand(string RefId) : IRequest<string>;

public sealed record DeleteJobCommand(string RefId) : IRequest<string>;