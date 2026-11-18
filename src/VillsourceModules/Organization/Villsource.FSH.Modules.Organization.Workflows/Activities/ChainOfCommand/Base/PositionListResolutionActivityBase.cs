using System.Text.Json.Nodes;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for chain of command resolvers returning a list of <see cref="ChainOfCommandPositionDto"/> as a <see cref="JsonObject"/> (<c>{ "items": [...], "count": N }</c>).
/// </summary>
public abstract class PositionListResolutionActivityBase : JsonListActivityBase
{
    [Output(Description = "First position in the list as a JsonObject, or null when empty.")]
    public Output<JsonObject?> FirstPosition { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ChainOfCommandPositionDto> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        SetJsonListResult(context, positions);
        FirstPosition.Set(context, WorkflowJsonHelper.ToJsonObject(positions.Count > 0 ? positions[0] : null));
    }
}
