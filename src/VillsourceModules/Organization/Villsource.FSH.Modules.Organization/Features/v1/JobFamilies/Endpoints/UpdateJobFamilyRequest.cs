namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Endpoints;

internal sealed record UpdateJobFamilyRequest(
    string Code,
    string Name,
    string IntlName,
    string? Description = null);
