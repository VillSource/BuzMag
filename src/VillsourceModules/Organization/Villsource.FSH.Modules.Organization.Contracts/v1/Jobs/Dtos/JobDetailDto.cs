namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

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