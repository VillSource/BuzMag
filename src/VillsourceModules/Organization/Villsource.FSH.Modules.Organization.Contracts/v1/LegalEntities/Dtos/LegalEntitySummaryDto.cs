namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;

public sealed record LegalEntitySummaryDto(
    string RefId,
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string? RegistrationNumber,
    string BaseCurrency,
    string? TradeName,
    string? LogoUrl,
    int ActiveBranchCount);
