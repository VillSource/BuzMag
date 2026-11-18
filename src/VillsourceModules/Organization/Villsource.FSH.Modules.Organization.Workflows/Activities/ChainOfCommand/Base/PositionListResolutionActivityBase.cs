using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for chain of command resolvers returning a list of <see cref="ChainOfCommandPositionDto"/>.
/// </summary>
public abstract class PositionListResolutionActivityBase : CodeActivity<IReadOnlyList<ChainOfCommandPositionDto>>
{
    [Output(Description = "Number of positions returned.")]
    public Output<int> Count { get; set; } = default!;

    [Output(Description = "First position in the list, or null when empty.")]
    public Output<ChainOfCommandPositionDto?> FirstPosition { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ChainOfCommandPositionDto> positions)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(positions);

        context.SetResult(positions);
        Count.Set(context, positions.Count);
        FirstPosition.Set(context, positions.Count > 0 ? positions[0] : null);
    }
}
