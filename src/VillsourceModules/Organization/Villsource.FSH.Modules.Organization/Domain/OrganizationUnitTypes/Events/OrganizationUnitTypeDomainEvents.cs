using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes.Events;

public sealed record OrganizationUnitTypeCreatedDomainEvent(
    Guid OrganizationUnitTypeId,
    string Code,
    string Name,
    int HierarchyLevel,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record OrganizationUnitTypeUpdatedDomainEvent(
    Guid OrganizationUnitTypeId,
    string Code,
    string Name,
    int HierarchyLevel,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record OrganizationUnitTypeActivatedDomainEvent(
    Guid OrganizationUnitTypeId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record OrganizationUnitTypeDeactivatedDomainEvent(
    Guid OrganizationUnitTypeId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record OrganizationUnitTypeDeletedDomainEvent(
    Guid OrganizationUnitTypeId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
