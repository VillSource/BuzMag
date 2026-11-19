using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for chain of command resolvers returning <see cref="IReadOnlyList{T}"/> of <see cref="ChainOfCommandPositionDto"/>.
/// </summary>
public abstract class PositionListResolutionActivityBase : ListActivityBase<ChainOfCommandPositionDto>
{
    [Output(Description = "First position in the list (ChainOfCommandPositionDto?), or null when empty.")]
    public Output<ChainOfCommandPositionDto?> FirstPosition { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ChainOfCommandPositionDto> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        SetListResult(context, positions);
        FirstPosition.Set(context, positions.Count > 0 ? positions[0] : null);
    }
}
