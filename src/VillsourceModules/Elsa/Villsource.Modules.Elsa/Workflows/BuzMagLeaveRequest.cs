using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;

namespace Villsource.Modules.Elsa.Workflows;

public class BuzMagLeaveRequest : BuzMagWorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.Build(builder);
        
        var helloLine = new WriteLine("Hello World!");

        builder.Root = new Flowchart { Activities =
        {
            helloLine
        }};
    }
}