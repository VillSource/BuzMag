namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

public sealed record JobSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? JobFamilyRefId,
    string? JobFamilyCode,
    string? JobFamilyName,
    string DefaultJobGradeRefId,
    string DefaultJobGradeCode,
    int DefaultJobGradeRankOrder,
    bool IsActive);