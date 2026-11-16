namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record PositionJobGradeDto(
    string RefId,
    string Code,
    string Name,
    int RankOrder);