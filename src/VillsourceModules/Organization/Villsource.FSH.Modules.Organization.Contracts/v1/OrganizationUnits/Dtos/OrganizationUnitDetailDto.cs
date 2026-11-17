using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitDetailDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    OrganizationUnitTypeLookupDto OrganizationUnitType,
    string? Description,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentCode,
    string? ParentName,
    int Depth,
    string HierarchyPath,
    string? DefaultCostCenterRefId,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool IsActive,
    IReadOnlyList<OrganizationUnitChildDto> Children,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);