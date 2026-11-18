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
    DisplayName = "Resolve Dotted-Line Manager",
    Description = "Resolves the dotted-line (matrix / functional) manager position for a given position."
)]
public class ResolveDottedLineManager : PositionResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the position whose dotted-line manager should be resolved.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Include Inactive",
        Description = "If true, returns the dotted-line manager position even if inactive or outside its validity window.",
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
        bool includeInactive = IncludeInactive.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        ChainOfCommandPositionDto? result = await service.ResolveDottedLineManagerAsync(
            positionRefId,
            includeInactive,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
