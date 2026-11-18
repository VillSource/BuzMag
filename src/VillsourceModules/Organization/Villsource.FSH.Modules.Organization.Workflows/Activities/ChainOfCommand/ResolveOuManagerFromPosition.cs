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
    DisplayName = "Resolve OU Manager From Position",
    Description = "Resolves the Organization Unit manager by starting at the position's OU and crawling up ancestor OUs."
)]
public class ResolveOuManagerFromPosition : OuManagerResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the starting position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Exclude Self",
        Description = "If true, skips any OU whose ManagerPosition equals the starting position and continues climbing.",
        DefaultValue = true
    )]
    public Input<bool> ExcludeSelf { get; set; } = new(true);

    [Input(
        DisplayName = "Include Inactive OU",
        Description = "If true, considers manager positions on inactive ancestor OUs.",
        DefaultValue = false
    )]
    public Input<bool> IncludeInactiveOu { get; set; } = new(false);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate manager position validity dates.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = ChainOfCommandActivityHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        bool excludeSelf = ExcludeSelf.GetOrDefault(context, static () => true);
        bool includeInactiveOu = IncludeInactiveOu.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        OuManagerResolutionDto? result = await service.ResolveOuManagerFromPositionAsync(
            positionRefId,
            excludeSelf,
            includeInactiveOu,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
