using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;


namespace Villsource.Modules.Elsa.Workflows;

public class HelloWorldWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Name = "HelloWorldWorkflow";

        // ประกาศ Signature ว่า Workflow นี้รับ Input ชื่อ "Message"
        builder.WithInput<string>("Message");
        builder.WithInput<string>("Tenant");

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("🚀 Hello from Elsa in FullStackHero!"),
                new WriteLine(context =>
                {
                    var message = context.GetInput<string>("Message") ?? "No Message Provided";
                    var t = context.GetInput<string>("Tenant") ?? "No Message Provided";

                    return $"🏢 Running for Tenant: {t ?? "Unknown"} | 📩 Message: {message}";
                })
            }
        };
    }
}