using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for effective manager/approver resolvers returning <see cref="EffectiveManagerResolutionDto"/> as JSON
/// and exposing both <see cref="EffectiveApproverPosition"/> and <see cref="NominalManagerPosition"/> as JSON.
/// </summary>
public abstract class EffectiveManagerResolutionActivityBase : CodeActivity<string?>
{
    [Output(Description = "The effective approver position as JSON (delegate if delegated; otherwise nominal manager).")]
    public Output<string?> EffectiveApproverPosition { get; set; } = default!;

    [Output(Description = "The nominal manager position in the hierarchy before delegation substitution as JSON.")]
    public Output<string?> NominalManagerPosition { get; set; } = default!;

    [Output(Description = "True when an active delegation substituted the nominal manager.")]
    public Output<bool> IsDelegated { get; set; } = default!;

    [Output(Description = "How the nominal manager position was resolved.")]
    public Output<string?> ResolutionSource { get; set; } = default!;

    [Output(Description = "Active delegation details as JSON when IsDelegated is true.")]
    public Output<string?> ActiveDelegation { get; set; } = default!;

    [Output(Description = "The OU ReferenceId where the manager was found when resolved via OU hierarchy.")]
    public Output<string?> ResolvedFromOrgUnitRefId { get; set; } = default!;

    [Output(Description = "True when an effective manager was resolved; otherwise false.")]
    public Output<bool> IsFound { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, EffectiveManagerResolutionDto? resolution)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetResult(ChainOfCommandActivityHelper.SerializeNullableJson(resolution));
        EffectiveApproverPosition.Set(context, ChainOfCommandActivityHelper.SerializeNullableJson(resolution?.EffectiveApproverPosition));
        NominalManagerPosition.Set(context, ChainOfCommandActivityHelper.SerializeNullableJson(resolution?.NominalManagerPosition));
        IsDelegated.Set(context, resolution?.IsDelegated ?? false);
        ResolutionSource.Set(context, resolution?.ResolutionSource.ToString());
        ActiveDelegation.Set(context, ChainOfCommandActivityHelper.SerializeNullableJson(resolution?.ActiveDelegation));
        ResolvedFromOrgUnitRefId.Set(context, resolution?.ResolvedFromOrgUnitRefId);
        IsFound.Set(context, resolution is not null);
    }
}
