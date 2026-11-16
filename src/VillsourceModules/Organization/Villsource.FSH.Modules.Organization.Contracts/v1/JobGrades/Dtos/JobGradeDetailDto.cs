namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

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