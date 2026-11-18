using System.Text.Json.Nodes;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.Modules.Elsa.Core.Activities.Base;

/// <summary>
/// Base activity for list resolvers returning a <see cref="JsonObject"/> (<c>{ "items": [...], "count": N }</c>)
/// along with a <see cref="Count"/> output.
/// </summary>
public abstract class JsonListActivityBase : CodeActivity<JsonObject>
{
    [Output(Description = "Number of items returned.")]
    public Output<int> Count { get; set; } = default!;

    protected void SetJsonListResult<T>(ActivityExecutionContext context, IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(items);

        context.SetResult(WorkflowJsonHelper.ToListResultJsonObject(items));
        Count.Set(context, items.Count);
    }
}
