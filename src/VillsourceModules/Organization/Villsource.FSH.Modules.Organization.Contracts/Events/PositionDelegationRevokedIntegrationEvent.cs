using FSH.Framework.Eventing.Abstractions;

namespace Villsource.FSH.Modules.Organization.Contracts.Events;

public sealed record PositionDelegationRevokedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    Guid PositionDelegationId,
    Guid LegalEntityId,
    Guid DelegatorPositionId,
    Guid DelegatePositionId) : IIntegrationEvent;
