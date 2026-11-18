using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for single-position chain of command resolvers returning <see cref="ChainOfCommandPositionDto"/> as JSON.
/// </summary>
public abstract class PositionResolutionActivityBase : CodeActivity<string?>
{
    [Output(Description = "True when a matching position was resolved; otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, ChainOfCommandPositionDto? position)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(ChainOfCommandActivityHelper.SerializeNullableJson(position));
        IsFound.Set(context, position is not null);
    }
}
