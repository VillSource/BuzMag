namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

public sealed record OrganizationUnitTypeDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int HierarchyLevel,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);
