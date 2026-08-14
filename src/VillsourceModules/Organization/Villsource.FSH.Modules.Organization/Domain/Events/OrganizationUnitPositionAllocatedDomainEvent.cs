using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Events;

public sealed record OrganizationUnitPositionAllocatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);