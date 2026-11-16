namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

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