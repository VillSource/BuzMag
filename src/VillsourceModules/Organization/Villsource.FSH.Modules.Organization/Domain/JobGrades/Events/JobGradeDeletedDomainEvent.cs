using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.JobGrades.Events
{
    public sealed record JobGradeDeletedDomainEvent(
        Guid JobGradeId,
        string Code,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}
