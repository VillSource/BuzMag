using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Jobs.Events;

public sealed record JobUpdatedDomainEvent(
    Guid JobId,
    string Code,
    string Name,
    Guid DefaultJobGradeId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);