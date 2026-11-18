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
/// Base activity for delegation resolvers returning a list of <see cref="ResolvedDelegationDto"/> as a <see cref="JsonObject"/> (<c>{ "items": [...], "count": N }</c>).
/// </summary>
public abstract class DelegationListResolutionActivityBase : JsonListActivityBase
{
    [Output(Description = "First (highest priority) delegation in the list as a JsonObject, or null when empty.")]
    public Output<JsonObject?> FirstDelegation { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ResolvedDelegationDto> delegations)
    {
        ArgumentNullException.ThrowIfNull(delegations);

        SetJsonListResult(context, delegations);
        FirstDelegation.Set(context, WorkflowJsonHelper.ToJsonObject(delegations.Count > 0 ? delegations[0] : null));
    }
}
