using FSH.Framework.Core.Domain;
namespace Villsource.FSH.Modules.Organization.Domain.JobGrades.Events
{
    public sealed record JobGradeCreatedDomainEvent(
        Guid JobGradeId,
        string Code,
        string Name,
        int RankOrder,
        Guid EventId,
        DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);
}
