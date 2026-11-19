using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.Modules.Elsa.Core.Activities.Base;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;

/// <summary>
/// Base activity for effective manager/approver resolvers returning <see cref="EffectiveManagerResolutionDto"/>
/// and exposing both <see cref="EffectiveApproverPosition"/> and <see cref="NominalManagerPosition"/> as <see cref="ChainOfCommandPositionDto"/>.
/// </summary>
public abstract class EffectiveManagerResolutionActivityBase : ObjectActivityBase<EffectiveManagerResolutionDto>
{
    [Output(Description = "The effective approver position (ChainOfCommandPositionDto?) — delegate if delegated; otherwise nominal manager.")]
    public Output<ChainOfCommandPositionDto?> EffectiveApproverPosition { get; set; } = default!;

    [Output(Description = "The nominal manager position in the hierarchy before delegation substitution (ChainOfCommandPositionDto?).")]
    public Output<ChainOfCommandPositionDto?> NominalManagerPosition { get; set; } = default!;

    [Output(Description = "True when an active delegation substituted the nominal manager (bool).")]
    public Output<bool> IsDelegated { get; set; } = default!;

    [Output(Description = "How the nominal manager position was resolved (ManagerResolutionSource?).")]
    public Output<ManagerResolutionSource?> ResolutionSource { get; set; } = default!;

    [Output(Description = "Active delegation details (ResolvedDelegationDto?) when IsDelegated is true; otherwise null.")]
    public Output<ResolvedDelegationDto?> ActiveDelegation { get; set; } = default!;

    [Output(Description = "The OU ReferenceId where the manager was found when resolved via OU hierarchy (string?).")]
    public Output<string?> ResolvedFromOrgUnitRefId { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, EffectiveManagerResolutionDto? resolution)
    {
        SetObjectResult(context, resolution);
        EffectiveApproverPosition.Set(context, resolution?.EffectiveApproverPosition);
        NominalManagerPosition.Set(context, resolution?.NominalManagerPosition);
        IsDelegated.Set(context, resolution?.IsDelegated ?? false);
        ResolutionSource.Set(context, resolution?.ResolutionSource);
        ActiveDelegation.Set(context, resolution?.ActiveDelegation);
        ResolvedFromOrgUnitRefId.Set(context, resolution?.ResolvedFromOrgUnitRefId);
    }
}
