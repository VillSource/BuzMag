using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for delegation resolvers returning <see cref="IReadOnlyList{T}"/> of <see cref="ResolvedDelegationDto"/>.
/// </summary>
public abstract class DelegationListResolutionActivityBase : ListActivityBase<ResolvedDelegationDto>
{
    [Output(Description = "First (highest priority) delegation in the list (ResolvedDelegationDto?), or null when empty.")]
    public Output<ResolvedDelegationDto?> FirstDelegation { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, IReadOnlyList<ResolvedDelegationDto> delegations)
    {
        ArgumentNullException.ThrowIfNull(delegations);

        SetListResult(context, delegations);
        FirstDelegation.Set(context, delegations.Count > 0 ? delegations[0] : null);
    }
}
