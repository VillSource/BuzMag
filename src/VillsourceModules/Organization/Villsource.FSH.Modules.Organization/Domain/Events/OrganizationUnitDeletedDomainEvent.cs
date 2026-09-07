using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Events;
public sealed record OrganizationUnitDeletedDomainEvent(
    Guid OrganizationUnitId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);