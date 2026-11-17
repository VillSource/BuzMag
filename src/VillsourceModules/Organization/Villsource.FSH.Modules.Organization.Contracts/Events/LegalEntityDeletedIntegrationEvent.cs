using FSH.Framework.Eventing.Abstractions;

namespace Villsource.FSH.Modules.Organization.Contracts.Events;

public sealed record LegalEntityDeletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    Guid LegalEntityId) : IIntegrationEvent;
