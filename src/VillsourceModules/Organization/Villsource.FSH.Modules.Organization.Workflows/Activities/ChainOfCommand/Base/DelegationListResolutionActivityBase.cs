using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for delegation resolvers returning a list of <see cref="ResolvedDelegationDto"/> as JSON.
/// </summary>
public abstract class DelegationListResolutionActivityBase : CodeActivity<string>
{
    [Output(Description = "Number of delegations returned.")]
    public Output<int> Count { get; set; } = default!;

    [Output(Description = "First (highest priority) delegation in the list as JSON, or null when empty.")]
    public Output<string?> FirstDelegation { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ResolvedDelegationDto> delegations)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(delegations);

        context.SetResult(ChainOfCommandActivityHelper.SerializeJson(delegations));
        Count.Set(context, delegations.Count);
        FirstDelegation.Set(context, ChainOfCommandActivityHelper.SerializeNullableJson(delegations.Count > 0 ? delegations[0] : null));
    }
}
