using Elsa.Common.Multitenancy;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using FSH.Framework.Eventing.Abstractions;
using Villsource.Modules.Elsa.Contracts.Constants;
using Villsource.Modules.Elsa.Contracts.Events;

namespace Villsource.Modules.Elsa.Activities;

public class PublishFinishStateEvent : CodeActivity
{
    public Input<string> ObjectId { get; init; } = null!;
    public Input<string> Result { get; init; } = null!;
    public Input<Dictionary<string, object>> Context { get; set; } = new([]);

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string? correlationId = context.WorkflowExecutionContext.CorrelationId;
        string workflowInstanceId = context.WorkflowExecutionContext.Id;

        var tenantAccessor = context.GetRequiredService<ITenantAccessor>();
        var timeProvider = context.GetRequiredService<TimeProvider>();
        var eventBus = context.GetRequiredService<IEventBus>();

        await eventBus.PublishAsync(new WorkflowFinishedIntegrationEvent(
            Id: Guid.CreateVersion7(),
            OccurredOnUtc: timeProvider.GetUtcNow().UtcDateTime,
            TenantId: tenantAccessor.TenantId,
            CorrelationId: correlationId ?? ObjectId.Get(context),
            Source: SourceName.Workflow,
            ObjectId: ObjectId.Get(context),
            InstanceId: workflowInstanceId,
            FlowDefinitionId: context.WorkflowExecutionContext.Workflow.Identity.DefinitionId,
            Result: Result.Get(context),
            Context: Context.Get(context)
        ), context.CancellationToken);
    }
}