namespace Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;

public sealed record OrganizationUnitLeadershipChainDto(
    string TargetOuRefId,
    string TargetOuCode,
    string TargetOuName,
    IReadOnlyList<OrganizationUnitLeadershipNodeDto> Chain);