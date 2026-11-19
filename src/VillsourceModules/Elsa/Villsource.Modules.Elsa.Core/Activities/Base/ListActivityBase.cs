using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Villsource.Modules.Elsa.Core.Activities.Base;

/// <summary>
/// Base activity for list resolvers returning <see cref="IReadOnlyList{T}"/> along with a <see cref="Count"/> output.
/// </summary>
public abstract class ListActivityBase<T> : CodeActivity<IReadOnlyList<T>>
{
    [Output(Description = "Number of items returned (int).")]
    public Output<int> Count { get; set; } = default!;

    protected void SetListResult(ActivityExecutionContext context, IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(items);

        context.SetResult(items);
        Count.Set(context, items.Count);
    }
}
