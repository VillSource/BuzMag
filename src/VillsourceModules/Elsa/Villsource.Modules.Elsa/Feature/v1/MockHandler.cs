using Elsa.Common.Models;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Mediator;
using Villsource.Modules.Elsa.Contracts.v1;
using Villsource.Modules.Elsa.Workflows;

namespace Villsource.Modules.Elsa.Feature.v1;

public class MockHandler(IWorkflowRunner runner, IWorkflowRuntime workflowRuntime, IWorkflowBuilderFactory workflowBuilderFactory):ICommandHandler<Mock, string>
{
    public async ValueTask<string> Handle(Mock command, CancellationToken cancellationToken)
    {
        var inlineWorkflow = new Sequence
        {
            Activities =
            {
                new WriteLine("Hello World!"),
                new WriteLine("We can do more than a one-liner!")
            }
        };
        await runner.RunAsync(inlineWorkflow, cancellationToken: cancellationToken);


        var builder = workflowBuilderFactory.CreateBuilder();
        
        var helloWorldDefinition = new HelloWorldWorkflow();
        
        var myWorkflow = await builder.BuildWorkflowAsync(helloWorldDefinition, cancellationToken);
        
        await runner.RunAsync(myWorkflow, cancellationToken: cancellationToken);
        
        
        // 2. สร้าง Client เพื่อเชื่อมต่อกับ Engine ส่วนกลาง
        var client = await workflowRuntime.CreateClientAsync(cancellationToken);
        
        // 3. เตรียมคำสั่งรัน โดยอ้างอิงผ่าน "ชื่อ" แทนการใช้ Type (Class)
        var request = new CreateAndRunWorkflowInstanceRequest
        {
            // สามารถส่งค่า Input ข้าม Module เข้าไปใน Workflow ได้ด้วย! (ถ้ามี)
            // Input = new Dictionary<string, object> { ["UserId"] = 123 },
            
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId("HelloWorldWorkflow") 
        };

        // 4. สั่งรันแบบ Asynchronous
        var result = await client.CreateAndRunInstanceAsync(request, cancellationToken);
        
        return $"Cross-Module Execution Success! Instance ID: {result.WorkflowInstanceId}";

    }
}