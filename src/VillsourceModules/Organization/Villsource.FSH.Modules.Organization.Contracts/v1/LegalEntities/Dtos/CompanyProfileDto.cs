namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

public sealed record CompanyProfileDto(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? ContactEmail,
    string? ContactPhone);