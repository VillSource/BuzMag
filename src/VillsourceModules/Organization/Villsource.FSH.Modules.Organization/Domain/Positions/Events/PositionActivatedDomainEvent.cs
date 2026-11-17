using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Positions.Events;

public sealed record PositionActivatedDomainEvent(
    Guid PositionId,
    Guid LegalEntityId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);