using FSH.Framework.Core.Domain;

namespace Villsource.FSH.Modules.Organization.Domain.OrganizationUnits.Events;

public sealed record OrganizationUnitMovedDomainEvent(
    Guid OrganizationUnitId,
    Guid? OldParentId,
    Guid? NewParentId,
    string NewHierarchyPath,
    int NewDepth,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : DomainEvent(EventId, OccurredOnUtc);