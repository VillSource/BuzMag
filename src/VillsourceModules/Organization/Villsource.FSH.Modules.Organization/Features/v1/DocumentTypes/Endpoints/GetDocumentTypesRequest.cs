namespace Villsource.FSH.Modules.Organization.Features.v1.DocumentTypes.Endpoints;

internal sealed record GetDocumentTypesRequest(
    string? Module = null,
    string? Keyword = null);
