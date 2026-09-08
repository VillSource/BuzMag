using FSH.Framework.Eventing.Abstractions;

namespace Villsource.Modules.Elsa.Contracts.Events;

public record ApprovalFinishedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    string ObjectId,
    string FlowIdentifier,
    string FinalDecision
) : IIntegrationEvent;