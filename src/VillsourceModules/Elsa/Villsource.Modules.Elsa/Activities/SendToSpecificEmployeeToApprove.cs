using Elsa.Common.Multitenancy;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using FSH.Framework.Eventing.Abstractions;
using Villsource.Modules.Elsa.Data;
using Villsource.Modules.Elsa.Domain;

namespace Villsource.Modules.Elsa.Activities;

public class SendToSpecificEmployeeToApprove : CodeActivity
{
    public Input<string> ObjectId { get; init; } = null!;
    public Input<string> RequesterId { get; init; } = null!;
    public Input<string> ReviewerId { get; init; } = null!;
    public Input<string[]> Options { get; init; } = null!;
    public Input<Dictionary<string, object>> Context { get; set; } = new([]);

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string workflowInstanceId = context.WorkflowExecutionContext.Id;
        string workflowDefinitionId = context.WorkflowExecutionContext.Workflow.Identity.DefinitionId;

        ElsaDbContext dbContext = context.GetRequiredService<ElsaDbContext>();

        ApprovalInbox approvalMessage = ApprovalInbox.Create(
            objectId: ObjectId.Get(context),
            requesterId: RequesterId.Get(context),
            reviewerId: ReviewerId.Get(context),
            workflowInstanceId: workflowInstanceId,
            workflowDefinitionId: workflowDefinitionId,
            allowedActions: Options.Get(context)
        );

        dbContext.ApprovalInbox.Add(approvalMessage);
        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}