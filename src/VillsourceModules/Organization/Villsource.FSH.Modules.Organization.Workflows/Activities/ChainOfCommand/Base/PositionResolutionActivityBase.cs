using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for single-position chain of command resolvers returning <see cref="ChainOfCommandPositionDto"/>.
/// </summary>
public abstract class PositionResolutionActivityBase : CodeActivity<ChainOfCommandPositionDto?>
{
    [Output(Description = "True when a matching position was resolved; otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, ChainOfCommandPositionDto? position)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(position);
        IsFound.Set(context, position is not null);
    }
}
