using Elsa.Workflows;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for single-position chain of command resolvers returning <see cref="ChainOfCommandPositionDto"/> as a <see cref="System.Text.Json.Nodes.JsonObject"/>.
/// </summary>
public abstract class PositionResolutionActivityBase : JsonObjectActivityBase
{
    protected void SetOutputs(ActivityExecutionContext context, ChainOfCommandPositionDto? position)
    {
        SetJsonResult(context, position);
    }
}
