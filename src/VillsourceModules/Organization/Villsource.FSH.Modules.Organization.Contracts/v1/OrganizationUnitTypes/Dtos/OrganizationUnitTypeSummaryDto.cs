namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

public sealed record OrganizationUnitTypeSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int HierarchyLevel,
    string? Description,
    bool IsActive);
