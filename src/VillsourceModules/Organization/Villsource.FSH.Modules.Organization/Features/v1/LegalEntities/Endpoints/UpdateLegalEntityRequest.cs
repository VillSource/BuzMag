namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record UpdateLegalEntityRequest(
    string Code,
    string LegalName,
    string IntlName,
    string TaxId,
    string BaseCurrency,
    string? RegNumber = null,
    int? FiscalYearStartMonth = null,
    string? TimeZoneId = null);
