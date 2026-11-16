namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

public sealed record JobGradeRefDto(
    string RefId,
    string Code,
    string Name,
    int RankOrder);