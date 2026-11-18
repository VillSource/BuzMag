using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Resolve Reporting Chain",
    Description = "Resolves the upward direct reporting chain of command from a starting position up to the top of the reporting tree."
)]
public class ResolveReportingChain : PositionListResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the starting position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Max Depth",
        Description = "Maximum number of upward reporting hops to traverse.",
        DefaultValue = 15,
        UIHint = InputUIHints.SingleLine
    )]
    public Input<int> MaxDepth { get; set; } = new(15);

    [Input(
        DisplayName = "Include Inactive",
        Description = "If true, includes inactive or expired positions in the returned chain.",
        DefaultValue = false
    )]
    public Input<bool> IncludeInactive { get; set; } = new(false);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate position validity windows.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = ChainOfCommandActivityHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        int maxDepth = MaxDepth.GetOrDefault(context, static () => 15);
        bool includeInactive = IncludeInactive.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<ChainOfCommandPositionDto> result = await service.ResolveReportingChainAsync(
            positionRefId,
            maxDepth,
            includeInactive,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
