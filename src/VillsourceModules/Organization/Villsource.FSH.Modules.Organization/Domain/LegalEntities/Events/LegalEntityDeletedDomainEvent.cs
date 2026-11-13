using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;

public sealed record LegalEntityDeletedDomainEvent(
    Guid LegalEntityId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);