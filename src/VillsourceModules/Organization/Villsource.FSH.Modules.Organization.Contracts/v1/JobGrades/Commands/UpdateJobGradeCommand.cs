using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;

public sealed record UpdateJobGradeCommand(
    string RefId,
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description = null,
    string? JobLevelCategory = null) : IRequest<string>;