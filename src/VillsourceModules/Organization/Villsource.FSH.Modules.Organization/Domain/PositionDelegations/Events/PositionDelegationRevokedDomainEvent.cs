using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.PositionDelegations.Events;

public sealed record PositionDelegationRevokedDomainEvent(
    Guid PositionDelegationId,
    Guid LegalEntityId,
    Guid DelegatorPositionId,
    Guid DelegatePositionId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);