using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;

public sealed record PositionDelegationUpdatedDomainEvent(
    Guid PositionDelegationId,
    Guid LegalEntityId,
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);