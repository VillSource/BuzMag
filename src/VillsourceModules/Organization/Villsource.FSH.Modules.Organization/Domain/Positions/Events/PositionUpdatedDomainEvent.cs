using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.Positions.Events;

public sealed record PositionUpdatedDomainEvent(
    Guid PositionId,
    Guid LegalEntityId,
    Guid OrgUnitId,
    Guid JobId,
    Guid JobGradeId,
    Guid? ReportsToPositionId,
    string Code,
    string Title,
    bool IsPrimaryLeadership,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);