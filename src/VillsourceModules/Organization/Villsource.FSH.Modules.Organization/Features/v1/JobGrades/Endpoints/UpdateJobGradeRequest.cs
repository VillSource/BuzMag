namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;

internal sealed record UpdateJobGradeRequest(
    string Code,
    string Name,
    string IntlName,
    int RankOrder,
    string? Description = null,
    string? Category = null);
