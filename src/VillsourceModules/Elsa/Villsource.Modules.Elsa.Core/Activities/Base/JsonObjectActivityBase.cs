using System.Text.Json.Nodes;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.Modules.Elsa.Core.Activities.Base;

/// <summary>
/// Base activity for single-object resolvers returning a <see cref="JsonObject"/> result and an <see cref="IsFound"/> flag.
/// </summary>
public abstract class JsonObjectActivityBase : CodeActivity<JsonObject?>
{
    [Output(Description = "True when a matching item was resolved; otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetJsonResult<T>(ActivityExecutionContext context, T? value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(WorkflowJsonHelper.ToJsonObject(value));
        IsFound.Set(context, value is not null);
    }
}
