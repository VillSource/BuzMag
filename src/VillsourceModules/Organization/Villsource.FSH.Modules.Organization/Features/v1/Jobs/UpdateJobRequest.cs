namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

internal sealed record UpdateJobRequest(
    string Code,
    string Name,
    string InternationalName,
    string DefaultJobGradeRefId,
    string? JobFamily = null,
    string? Description = null);