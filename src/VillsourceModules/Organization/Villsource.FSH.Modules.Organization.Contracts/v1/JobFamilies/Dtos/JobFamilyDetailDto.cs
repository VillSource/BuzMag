namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

public sealed record JobFamilyDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);
