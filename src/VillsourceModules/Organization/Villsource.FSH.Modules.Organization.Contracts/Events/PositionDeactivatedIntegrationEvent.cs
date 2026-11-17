using FSH.Framework.Eventing.Abstractions;

namespace Villsource.FSH.Modules.Organization.Contracts.Events;

public sealed record PositionDeactivatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    Guid PositionId,
    Guid LegalEntityId,
    string Code) : IIntegrationEvent;
