namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

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
