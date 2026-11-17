using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;

public sealed record OrganizationUnitCreatedDomainEvent(
    Guid OrganizationUnitId,
    string Code,
    string Name,
    string InternationalName,
    string Type,
    Guid? LegalEntityId,
    Guid? ParentId,
    Guid? ManagerPositionId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);