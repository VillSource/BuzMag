using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Villsource.Modules.Elsa.Core.Activities.Base;

/// <summary>
/// Base activity for boolean condition checks that route Flowchart workflows to <c>True</c> or <c>False</c> outcomes
/// while also exposing the boolean <see cref="Result"/> output.
/// </summary>
[FlowNode("True", "False")]
public abstract class ConditionActivityBase : Activity
{
    [Output(Description = "Boolean evaluation result (true or false).")]
    public Output<bool> Result { get; set; } = default!;

    protected async ValueTask CompleteConditionAsync(ActivityExecutionContext context, bool result)
    {
        ArgumentNullException.ThrowIfNull(context);

        Result.Set(context, result);
        await context.CompleteActivityWithOutcomesAsync(result ? "True" : "False").ConfigureAwait(false);
    }
}
