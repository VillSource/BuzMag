using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;

public sealed record OrganizationUnitManagerPositionChangedDomainEvent(
    Guid OrganizationUnitId,
    Guid? PreviousManagerPositionId,
    Guid? NewManagerPositionId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);