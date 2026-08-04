using FSH.Framework.Eventing.Abstractions;

namespace Villsource.FSH.Modules.Organization.Contracts.Events;

public sealed record OrganizationUnitCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source
    ): IIntegrationEvent;