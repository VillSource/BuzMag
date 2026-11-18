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
    DisplayName = "Get Direct Reports",
    Description = "Retrieves all immediate direct report positions for a given manager position."
)]
public class GetDirectReports : PositionListResolutionActivityBase
{
    [Input(
        DisplayName = "Manager Position Ref ID",
        Description = "The ReferenceId of the manager position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> ManagerPositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Include Inactive",
        Description = "If true, includes inactive direct report positions.",
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
        string managerPositionRefId = ManagerPositionRefId.Get(context);
        bool includeInactive = IncludeInactive.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<ChainOfCommandPositionDto> result = await service.GetDirectReportsAsync(
            managerPositionRefId,
            includeInactive,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
