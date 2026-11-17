namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitChildDto(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    string OrganizationUnitTypeCode,
    string OrganizationUnitTypeName,
    int Depth,
    string? ManagerPositionRefId,
    string? ManagerPositionTitle,
    bool IsActive);