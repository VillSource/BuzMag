using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;

public sealed record OrganizationUnitUpdatedDomainEvent(
    Guid OrganizationUnitId,
    string Code,
    string Name,
    string InternationalName,
    Guid OrganizationUnitTypeId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);