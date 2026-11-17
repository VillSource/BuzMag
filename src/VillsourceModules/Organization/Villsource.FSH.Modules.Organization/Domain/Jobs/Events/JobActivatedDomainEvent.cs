using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Jobs.Events;

public sealed record JobActivatedDomainEvent(
    Guid JobId,
    string Code,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);