using FSH.Framework.Eventing.Abstractions;

namespace Villsource.FSH.Modules.Organization.Contracts.Events;

public sealed record OrganizationUnitMovedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    Guid OrganizationUnitId,
    Guid? OldParentId,
    Guid? NewParentId,
    string NewHierarchyPath,
    int NewLevel) : IIntegrationEvent;
