namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

public sealed record DelegationPositionDto(
    string RefId,
    string Code,
    string Title,
    string JobGradeCode,
    int JobGradeRankOrder);