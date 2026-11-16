using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;

public sealed record CreateJobGradeCommand(
    string Code,
    string Name,
    string InternationalName,
    int RankOrder,
    string? Description = null,
    bool IsActive = true) : IRequest<string>;