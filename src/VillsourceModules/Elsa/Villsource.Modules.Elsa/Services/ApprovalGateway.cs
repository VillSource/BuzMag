using Elsa.Common.Models;
using Elsa.Extensions;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Villsource.Modules.Elsa.Contracts.Dtos;
using Villsource.Modules.Elsa.Contracts.Services;
using Villsource.Modules.Elsa.Contracts.v1.Approval;
using Villsource.Modules.Elsa.Data;
using Villsource.Modules.Elsa.Domain;

namespace Villsource.Modules.Elsa.Services;

public class ApprovalGateway(
    IWorkflowRuntime workflowRuntime,
    IWorkflowDefinitionStore workflowDefinitionStore)
    : IApprovalGateway
{
    public async Task<StartWorkflowResult> StartWorkflowAsync(SubmitToApprovalCommand data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        WorkflowDefinitionFilter filter = new()
        {
            DefinitionId = data.FlowDefinitionId,
            VersionOptions = VersionOptions.Published,
        };

        if (await workflowDefinitionStore.FindAsync(filter, cancellationToken) is null)
            return StartWorkflowResult.Error($"Workflow Definition with ID {data.FlowDefinitionId} does not exist");

        data.ContextData.AddRange(new Dictionary<string, object>
        {
            [nameof(data.ObjectId)] = data.ObjectId,
            [nameof(data.RequestedOn)] = data.RequestedOn,
            [nameof(data.RequesterId)] = data.RequesterId,
            [nameof(data.Resource)] = data.Resource,
        });
        CreateAndRunWorkflowInstanceRequest request = new()
        {
            Input = data.ContextData,
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(data.FlowDefinitionId),
        };

        IWorkflowClient client = await workflowRuntime.CreateClientAsync(cancellationToken);
        RunWorkflowInstanceResponse result = await client.CreateAndRunInstanceAsync(request, cancellationToken);
        return StartWorkflowResult.Success(result.WorkflowInstanceId);
    }
}