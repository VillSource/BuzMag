using Elsa.Common.Multitenancy;
using FSH.Framework.Eventing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Villsource.Modules.Elsa.Contracts.Events;
using Villsource.Modules.Elsa.Data;

namespace Villsource.Modules.Elsa.IntegrationEvents;

public partial class TrapFinishedWorkflowForTest(
    ElsaDbContext dbContext,
    ILogger<TrapFinishedWorkflowForTest> logger)
    : IIntegrationEventHandler<WorkflowFinishedIntegrationEvent>
{
    public async Task HandleAsync(WorkflowFinishedIntegrationEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var x = await dbContext.ApprovalInbox
            // .Where(i => @event.ObjectId == i.ObjectId)
            .Where(i => @event.InstanceId == i.WorkflowInstanceId)
            .Where(i => !i.IsFinished)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        foreach (var i in x)
        {
            i.MarkAsFinish();
            LogObjObj2(i.ObjectId, @event.ObjectId);
            LogObjObj2(i.WorkflowInstanceId, @event.InstanceId);
        }

        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        LogWorkflowFinishedTrapEventForTestWithDateEvent(@event);
    }

    [LoggerMessage(LogLevel.Information, "[Workflow Finished] Trap event for test with date -->\n{Event}")]
    partial void LogWorkflowFinishedTrapEventForTestWithDateEvent(WorkflowFinishedIntegrationEvent @event);

    [LoggerMessage(LogLevel.Information, "{Obj}:{Obj2}")]
    partial void LogObjObj2(string obj, string obj2);
}