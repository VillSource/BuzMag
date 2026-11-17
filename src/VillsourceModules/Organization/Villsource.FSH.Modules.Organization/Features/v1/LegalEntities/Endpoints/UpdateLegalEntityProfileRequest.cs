namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

internal sealed record UpdateLegalEntityProfileRequest(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone);