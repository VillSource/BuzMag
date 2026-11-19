using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Villsource.Modules.Elsa.Core.Activities.Base;

/// <summary>
/// Base activity for single-object resolvers returning <typeparamref name="T"/> and an <see cref="IsFound"/> flag.
/// </summary>
public abstract class ObjectActivityBase<T> : CodeActivity<T?>
    where T : class
{
    [Output(Description = "True when a matching item was resolved (bool); otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetObjectResult(ActivityExecutionContext context, T? value)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(value);
        IsFound.Set(context, value is not null);
    }
}
