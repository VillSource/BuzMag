using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Workflows.Constants;
using Villsource.Modules.Elsa.Core.Activities.Base;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Is In Chain Of Command",
    Description = "Determines whether a candidate supervisor position holds supervisory authority over a subordinate position (returns bool)."
)]
public class IsInChainOfCommand : ConditionActivityBase
{
    [Input(
        DisplayName = "Supervisor Position Ref ID",
        Description = "The ReferenceId of the candidate supervisor or manager position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> SupervisorPositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Subordinate Position Ref ID",
        Description = "The ReferenceId of the subordinate position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> SubordinatePositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Include Dotted-Line",
        Description = "If true, also checks the subordinate's immediate dotted-line manager.",
        DefaultValue = false
    )]
    public Input<bool> IncludeDottedLine { get; set; } = new(false);

    [Input(
        DisplayName = "Include OU Hierarchy",
        Description = "If true, also checks ancestor OU manager positions above the subordinate.",
        DefaultValue = true
    )]
    public Input<bool> IncludeOuHierarchy { get; set; } = new(true);

    [Input(
        DisplayName = "Include Active Delegations",
        Description = "If true, returns true when the supervisor holds an active delegation for any manager in the subordinate's chain.",
        DefaultValue = true
    )]
    public Input<bool> IncludeActiveDelegations { get; set; } = new(true);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate validity and active delegations.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string supervisorPositionRefId = WorkflowJsonHelper.RequireNonEmpty(SupervisorPositionRefId.GetOrDefault(context), nameof(SupervisorPositionRefId));
        string subordinatePositionRefId = WorkflowJsonHelper.RequireNonEmpty(SubordinatePositionRefId.GetOrDefault(context), nameof(SubordinatePositionRefId));
        bool includeDottedLine = IncludeDottedLine.GetOrDefault(context);
        bool includeOuHierarchy = IncludeOuHierarchy.GetOrDefault(context, static () => true);
        bool includeActiveDelegations = IncludeActiveDelegations.GetOrDefault(context, static () => true);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        bool result = await service.IsInChainOfCommandAsync(
            supervisorPositionRefId,
            subordinatePositionRefId,
            includeDottedLine,
            includeOuHierarchy,
            includeActiveDelegations,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        await CompleteConditionAsync(context, result).ConfigureAwait(false);
    }
}
