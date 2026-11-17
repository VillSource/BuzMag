namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs.Endpoints;

internal sealed record CreateJobRequest(
    string Code,
    string Name,
    string IntlName,
    string JobGradeRefId,
    string? FamilyRefId = null,
    string? Description = null,
    bool IsActive = true);
