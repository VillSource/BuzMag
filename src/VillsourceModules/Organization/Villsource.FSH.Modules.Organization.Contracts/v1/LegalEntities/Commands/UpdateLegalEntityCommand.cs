using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Commands;

public sealed record UpdateLegalEntityCommand(
    string RefId,
    string Code,
    string LegalName,
    string InternationalName,
    string TaxId,
    string BaseCurrency,
    string? RegistrationNumber = null,
    int? FiscalYearStartMonth = null,
    string? TimeZoneId = null) : IRequest<string>;