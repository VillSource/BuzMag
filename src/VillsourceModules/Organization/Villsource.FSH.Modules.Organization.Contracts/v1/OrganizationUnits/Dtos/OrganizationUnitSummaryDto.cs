namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitSummaryDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    string? LegalEntityRefId,
    string? ParentRefId,
    string? ParentName,
    int Level,
    string HierarchyPath,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool IsActive,
    int ActiveChildCount);
