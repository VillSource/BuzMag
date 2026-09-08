using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using FSH.Framework.Core.Context;
using Mediator;
using Villsource.Modules.Elsa.Contracts.v1;

namespace Villsource.Modules.Elsa.Feature.v1;

public class MockHandler(IWorkflowRuntime workflowRuntime, ICurrentUser user) : ICommandHandler<Mock, string>
{
    public async ValueTask<string> Handle(Mock command, CancellationToken cancellationToken)
    {
        var client = await workflowRuntime.CreateClientAsync(cancellationToken);

        var request = new CreateAndRunWorkflowInstanceRequest
        {
            Input = new Dictionary<string, object>
            {
                ["Message"] = "Pandora",
                ["Tenant"] = user.GetTenant() ?? string.Empty
            },
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId("HelloWorldWorkflow")
        };

        var result = await client.CreateAndRunInstanceAsync(request, cancellationToken);
        return $"Cross-Module Execution Success! Instance ID: {result.WorkflowInstanceId}";
    }
}