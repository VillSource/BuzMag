using Elsa.Workflows;
using Elsa.Workflows.Activities;
using FSH.Framework.Shared.Multitenancy;

namespace Villsource.Modules.Elsa.Workflows;

public class HelloWorldWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Name = "HelloWorldWorkflow";
        builder.Id = "eiei";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("🚀 Hello from Elsa in FullStackHero!"),
                // ทดสอบดึง TenantId ออกมาดูเพื่อยืนยันว่าระบบ Tenant ทำงานถูกต้อง
                new WriteLine(context =>
                {
                    var tenantInfo = "test";
                    return $"🏢 Running for Tenant: {tenantInfo}";
                })
            }
        };
    }
}