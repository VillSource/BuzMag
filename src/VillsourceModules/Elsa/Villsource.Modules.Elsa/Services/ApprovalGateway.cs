using Elsa.Common.Models;
using Elsa.Extensions;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using FluentValidation;
using FluentValidation.Results;
using Villsource.Modules.Elsa.Contracts.Dtos;
using Villsource.Modules.Elsa.Contracts.Services;
using Villsource.Modules.Elsa.Contracts.v1.Approval;

namespace Villsource.Modules.Elsa.Services;

public class ApprovalGateway(
    IWorkflowRuntime workflowRuntime,
    IWorkflowDefinitionStore workflowDefinitionStore)
    : IApprovalGateway
{
    public async Task<StartWorkflowResult> StartNewWorkflowAsync(SubmitToApprovalCommand data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        WorkflowDefinitionFilter filter = new()
        {
            DefinitionId = data.FlowDefinitionId, VersionOptions = VersionOptions.Published,
        };

        var workflow = await workflowDefinitionStore.FindAsync(filter, cancellationToken);
        if (workflow is null)
            return StartWorkflowResult.Error($"Workflow Definition with ID {data.FlowDefinitionId} does not exist");
        var inputs = workflow.Inputs.Select(i => i.Name);

        data.ContextData.Add(nameof(data.ObjectId), data.ObjectId);
        data.ContextData.Add(nameof(data.RequestedOn), data.RequestedOn);
        data.ContextData.Add(nameof(data.RequesterId), data.RequesterId);
        data.ContextData.Add(nameof(data.ObjectName), data.ObjectName);

        var requiedInputs = inputs
            .Where(i => !data.ContextData.Keys.Contains(i, StringComparer.OrdinalIgnoreCase))
            .Select(i => new ValidationFailure(i, "Required for workflow input"))
            .ToArray();
        
        if (requiedInputs.Length > 0)
            return StartWorkflowResult.Error("Required inputs are not provided",
                new ValidationException("", requiedInputs));

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