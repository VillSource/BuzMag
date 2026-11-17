namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades
{
    internal sealed record UpdateJobGradeRequest(
        string Code,
        string Name,
        string InternationalName,
        int RankOrder,
        string? Description = null);

}