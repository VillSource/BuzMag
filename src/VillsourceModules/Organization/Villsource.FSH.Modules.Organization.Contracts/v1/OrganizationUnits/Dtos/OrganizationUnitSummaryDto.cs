namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string OrganizationUnitTypeRefId,
    string OrganizationUnitTypeCode,
    string OrganizationUnitTypeName,
    int OrganizationUnitTypeHierarchyLevel,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentName,
    int Depth,
    string HierarchyPath,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool IsActive,
    int ActiveChildCount);
