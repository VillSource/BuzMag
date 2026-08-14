using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Events;

public sealed record OrganizationUnitUpdatedDomainEvent(
    Guid OrganizationId,
    Guid OrganizationUnitId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);