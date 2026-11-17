namespace Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Endpoints;

internal sealed record DocumentTypeResponse(
    string Code,
    string Name,
    string Module,
    string? Description);
