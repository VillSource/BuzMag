using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record CreateLegalEntityCommand(
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null,
    CompanyProfileDto? Profile = null,
    Address? HeadOfficeAddress = null,
    string? HeadOfficeBranchCode = null,
    string? HeadOfficeName = null,
    string? HeadOfficeInternationalName = null,
    DateTimeOffset? HeadOfficeRegisteredDate = null,
    int? FiscalYearStartMonth = null,
    string? TimeZoneId = null) : IRequest<string>;