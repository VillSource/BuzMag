namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record PositionNodeDto(
    string RefId,
    string Code,
    string Title,
    string InternationalTitle,
    string JobGradeCode,
    int JobGradeRankOrder,
    bool IsPrimaryLeadership,
    bool IsActive);