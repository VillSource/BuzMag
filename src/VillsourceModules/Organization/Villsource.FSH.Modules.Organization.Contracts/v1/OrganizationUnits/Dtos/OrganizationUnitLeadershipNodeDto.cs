namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitLeadershipNodeDto(
    int DepthFromTarget,
    int OuLevel,
    string OuRefId,
    string OuCode,
    string OuName,
    bool OuIsActive,
    string? ManagerPositionRefId,
    string? ManagerPositionCode,
    string? ManagerPositionTitle,
    bool? ManagerPositionIsActive);