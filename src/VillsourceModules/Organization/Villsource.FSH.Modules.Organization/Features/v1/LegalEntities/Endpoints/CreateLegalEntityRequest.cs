using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record CreateLegalEntityRequest(
    string Code,
    string CountryCode,
    string LegalName,
    string IntlName,
    string TaxId,
    string BaseCurrency,
    string? RegNumber = null,
    CompanyProfileRequest? Profile = null,
    Address? HoAddress = null,
    string? HoBranchCode = null,
    string? HoName = null,
    string? HoIntlName = null,
    DateTimeOffset? HoRegisteredDate = null,
    int? FiscalYearStartMonth = null,
    string? TimeZoneId = null);

internal sealed record CompanyProfileRequest(
    string? TradeName = null,
    string? LogoUrl = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null);
