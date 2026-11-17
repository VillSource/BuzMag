namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

internal sealed record UpdateLegalEntityRequest(
    string Code,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null,
    int? FiscalYearStartMonth = null,
    string? TimeZoneId = null);