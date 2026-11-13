using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record LegalEntityUpdatedDomainEvent(
    Guid LegalEntityId,
    string Code,
    string CountryCode,
    string LegalName,
    string InternationalName,
    string TaxId,
    string? RegistrationNumber,
    string BaseCurrency,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);