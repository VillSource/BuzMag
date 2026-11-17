namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record UpdateLegalEntityProfileRequest(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? Email,
    string? Phone);
