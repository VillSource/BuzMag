using Elsa.Workflows;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for single-position chain of command resolvers returning <see cref="ChainOfCommandPositionDto"/>.
/// </summary>
public abstract class PositionResolutionActivityBase : ObjectActivityBase<ChainOfCommandPositionDto>
{
    protected void SetOutputs(ActivityExecutionContext context, ChainOfCommandPositionDto? position)
    {
        SetObjectResult(context, position);
    }
}
