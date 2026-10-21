using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.Modules.HumanResource.Workflow.Contract.Constants;

namespace Villsource.Modules.HumanResource.Workflow.Activities;

[Activity(
    Category = HrActivityConstant.Category
)]
public class MockStrOutput : CodeActivity<string>
{
    [Input(
        UIHint = InputUIHints.SingleLine,
        DefaultValue = "TEMP-STR-001"
    )]
    public Input<string> str { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var chosenOption = str.Get(context);
        context.SetResult(chosenOption);
    }
}