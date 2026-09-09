using FSH.Framework.Eventing.Abstractions;

namespace Villsource.Modules.Elsa.Contracts.Events;

public record WorkflowFinishedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? TenantId,
    string CorrelationId,
    string Source,
    string ObjectId,
    string InstanceId,
    string FlowDefinitionId,
    string Result,
    Dictionary<string,object>? Context = null) : IIntegrationEvent;