using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for effective manager/approver resolvers returning <see cref="EffectiveManagerResolutionDto"/>
/// and exposing both <see cref="EffectiveApproverPosition"/> and <see cref="NominalManagerPosition"/> directly.
/// </summary>
public abstract class EffectiveManagerResolutionActivityBase : CodeActivity<EffectiveManagerResolutionDto?>
{
    [Output(Description = "The effective approver position (delegate if delegated; otherwise nominal manager).")]
    public Output<ChainOfCommandPositionDto?> EffectiveApproverPosition { get; set; } = default!;

    [Output(Description = "The nominal manager position in the hierarchy before delegation substitution.")]
    public Output<ChainOfCommandPositionDto?> NominalManagerPosition { get; set; } = default!;

    [Output(Description = "True when an active delegation substituted the nominal manager.")]
    public Output<bool> IsDelegated { get; set; } = default!;

    [Output(Description = "How the nominal manager position was resolved.")]
    public Output<ManagerResolutionSource?> ResolutionSource { get; set; } = default!;

    [Output(Description = "Active delegation details when IsDelegated is true.")]
    public Output<ResolvedDelegationDto?> ActiveDelegation { get; set; } = default!;

    [Output(Description = "The OU ReferenceId where the manager was found when resolved via OU hierarchy.")]
    public Output<string?> ResolvedFromOrgUnitRefId { get; set; } = default!;

    [Output(Description = "True when an effective manager was resolved; otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, EffectiveManagerResolutionDto? resolution)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(resolution);
        EffectiveApproverPosition.Set(context, resolution?.EffectiveApproverPosition);
        NominalManagerPosition.Set(context, resolution?.NominalManagerPosition);
        IsDelegated.Set(context, resolution?.IsDelegated ?? false);
        ResolutionSource.Set(context, resolution?.ResolutionSource);
        ActiveDelegation.Set(context, resolution?.ActiveDelegation);
        ResolvedFromOrgUnitRefId.Set(context, resolution?.ResolvedFromOrgUnitRefId);
        IsFound.Set(context, resolution is not null);
    }
}
