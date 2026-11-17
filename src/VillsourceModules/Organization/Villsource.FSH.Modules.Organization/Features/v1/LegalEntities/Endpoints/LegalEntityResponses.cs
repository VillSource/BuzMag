using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record LegalEntitySummaryResponse(
    string RefId,
    string Code,
    string CountryCode,
    string LegalName,
    string IntlName,
    string TaxId,
    string? RegNumber,
    string BaseCurrency,
    string? TradeName,
    string? LogoUrl,
    int ActiveBranchCount);

internal sealed record LegalEntityDetailResponse(
    string RefId,
    string Code,
    string CountryCode,
    string LegalName,
    string IntlName,
    string TaxId,
    string? RegNumber,
    string BaseCurrency,
    int? FiscalYearStartMonth,
    string? TimeZoneId,
    CompanyProfileResponse Profile,
    IReadOnlyList<TaxBranchResponse> Branches,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);

internal sealed record CompanyProfileResponse(
    string? TradeName,
    string? LogoUrl,
    string? Website,
    string? Email,
    string? Phone);

internal sealed record TaxBranchResponse(
    string RefId,
    string LegalEntityRefId,
    string BranchCode,
    bool IsHeadOffice,
    string Name,
    string IntlName,
    string? TaxSubId,
    Address Address,
    bool IsActive,
    DateTimeOffset? RegisteredDate,
    DateTimeOffset? ClosedDate);
