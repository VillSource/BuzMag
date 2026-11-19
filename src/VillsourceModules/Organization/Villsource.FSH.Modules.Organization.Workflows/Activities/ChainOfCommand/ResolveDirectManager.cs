using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;
using Villsource.FSH.Modules.Organization.Workflows.Constants;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Resolve Direct Manager",
    Description = "Resolves the direct (solid-line) reporting manager position for a given position (returns ChainOfCommandPositionDto?)."
)]
public class ResolveDirectManager : PositionResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the subordinate position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Climb To First Active",
        Description = "If true, walks up the reporting line past inactive or expired manager positions until the first active manager is found.",
        DefaultValue = false
    )]
    public Input<bool> ClimbToFirstActive { get; set; } = new(false);

    [Input(
        DisplayName = "Include Inactive",
        Description = "If true, returns the immediate manager position even if inactive or outside its validity window.",
        DefaultValue = false
    )]
    public Input<bool> IncludeInactive { get; set; } = new(false);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate position validity dates.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = WorkflowJsonHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        bool climbToFirstActive = ClimbToFirstActive.GetOrDefault(context);
        bool includeInactive = IncludeInactive.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        ChainOfCommandPositionDto? result = await service.ResolveDirectManagerAsync(
            positionRefId,
            climbToFirstActive,
            includeInactive,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
