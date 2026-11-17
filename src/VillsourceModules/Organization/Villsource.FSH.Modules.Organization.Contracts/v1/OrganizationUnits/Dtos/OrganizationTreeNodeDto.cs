namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationTreeNodeDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    int Level,
    string HierarchyPath,
    bool IsActive,
    string? DefaultCostCenterRefId,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    IReadOnlyList<OrganizationTreeNodeDto> Children);
