using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.JobFamilies.Events;

public sealed record JobFamilyCreatedDomainEvent(
    Guid JobFamilyId,
    string Code,
    string Name,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record JobFamilyUpdatedDomainEvent(
    Guid JobFamilyId,
    string Code,
    string Name,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record JobFamilyActivatedDomainEvent(
    Guid JobFamilyId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record JobFamilyDeactivatedDomainEvent(
    Guid JobFamilyId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);

public sealed record JobFamilyDeletedDomainEvent(
    Guid JobFamilyId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
