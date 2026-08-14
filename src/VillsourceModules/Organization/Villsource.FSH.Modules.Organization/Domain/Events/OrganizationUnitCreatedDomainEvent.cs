using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Events;

public sealed record OrganizationUnitCreatedDomainEvent(
    Guid Id,
    string ReferenceId,
    string Code,
    string Name,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);