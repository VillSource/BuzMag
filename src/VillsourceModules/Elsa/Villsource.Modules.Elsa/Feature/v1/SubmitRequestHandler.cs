using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Mediator;

namespace Villsource.Modules.Elsa.Feature.v1;

public record SubmitRequestCommand(string Name, string ReqId) : IRequest<string>;

public class SubmitRequestHandler(IWorkflowRuntime workflowRuntime) : IRequestHandler<SubmitRequestCommand, string>
{
    public async ValueTask<string> Handle(SubmitRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var client = await workflowRuntime.CreateClientAsync(cancellationToken);

        var request = new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId("DocumentApprovalWorkflow"),
            
            // 💡 1. ติดป้ายชื่อให้ Workflow นี้ด้วย ReqId
            CorrelationId = command.ReqId, 
            
            Input = new Dictionary<string, object> 
            { 
                ["Name"] = command.Name,
                ["ReqId"] = command.ReqId
            }
        };

        // 💡 2. สั่งรัน (Workflow จะวิ่งไปติดที่ Activity 'Event' แล้วหยุดพัก)
        await client.CreateAndRunInstanceAsync(request, cancellationToken);
        
        return $"Workflow started! ReqId: {command.ReqId} is now Paused.";
    }
}