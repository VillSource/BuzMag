using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;

public sealed record OrganizationUnitUpdatedDomainEvent(
    Guid OrganizationUnitId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);