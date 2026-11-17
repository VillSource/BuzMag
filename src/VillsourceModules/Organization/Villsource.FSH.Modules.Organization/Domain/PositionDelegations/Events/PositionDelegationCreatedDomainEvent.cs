using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;

public sealed record PositionDelegationCreatedDomainEvent(
    Guid PositionDelegationId,
    Guid LegalEntityId,
    Guid DelegatorPositionId,
    Guid DelegatePositionId,
    string DelegationType,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);