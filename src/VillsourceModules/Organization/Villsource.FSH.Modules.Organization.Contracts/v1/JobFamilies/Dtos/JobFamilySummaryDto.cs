namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

public sealed record JobFamilySummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string? Description,
    bool IsActive);
