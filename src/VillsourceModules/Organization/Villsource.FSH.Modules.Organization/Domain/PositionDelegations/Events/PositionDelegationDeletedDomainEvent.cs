using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;

public sealed record PositionDelegationDeletedDomainEvent(
    Guid PositionDelegationId,
    Guid LegalEntityId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);