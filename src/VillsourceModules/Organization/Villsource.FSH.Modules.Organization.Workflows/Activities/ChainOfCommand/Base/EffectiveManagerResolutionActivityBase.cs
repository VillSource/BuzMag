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
/// Base activity for effective manager/approver resolvers returning <see cref="EffectiveManagerResolutionDto"/> as a <see cref="JsonObject"/>
/// and exposing both <see cref="EffectiveApproverPosition"/> and <see cref="NominalManagerPosition"/> as <see cref="JsonObject"/>.
/// </summary>
public abstract class EffectiveManagerResolutionActivityBase : JsonObjectActivityBase
{
    [Output(Description = "The effective approver position as a JsonObject (delegate if delegated; otherwise nominal manager).")]
    public Output<JsonObject?> EffectiveApproverPosition { get; set; } = default!;

    [Output(Description = "The nominal manager position in the hierarchy before delegation substitution as a JsonObject.")]
    public Output<JsonObject?> NominalManagerPosition { get; set; } = default!;

    [Output(Description = "True when an active delegation substituted the nominal manager.")]
    public Output<bool> IsDelegated { get; set; } = default!;

    [Output(Description = "How the nominal manager position was resolved.")]
    public Output<string?> ResolutionSource { get; set; } = default!;

    [Output(Description = "Active delegation details as a JsonObject when IsDelegated is true.")]
    public Output<JsonObject?> ActiveDelegation { get; set; } = default!;

    [Output(Description = "The OU ReferenceId where the manager was found when resolved via OU hierarchy.")]
    public Output<string?> ResolvedFromOrgUnitRefId { get; set; } = default!;

    protected void SetOutputs(ActivityExecutionContext context, EffectiveManagerResolutionDto? resolution)
    {
        SetJsonResult(context, resolution);
        EffectiveApproverPosition.Set(context, WorkflowJsonHelper.ToJsonObject(resolution?.EffectiveApproverPosition));
        NominalManagerPosition.Set(context, WorkflowJsonHelper.ToJsonObject(resolution?.NominalManagerPosition));
        IsDelegated.Set(context, resolution?.IsDelegated ?? false);
        ResolutionSource.Set(context, resolution?.ResolutionSource.ToString());
        ActiveDelegation.Set(context, WorkflowJsonHelper.ToJsonObject(resolution?.ActiveDelegation));
        ResolvedFromOrgUnitRefId.Set(context, resolution?.ResolvedFromOrgUnitRefId);
    }
}
