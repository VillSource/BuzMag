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
    DisplayName = "Resolve OU Manager From Org Unit",
    Description = "Resolves the Organization Unit manager by starting at an Organization Unit and crawling up ancestor OUs."
)]
public class ResolveOuManagerFromOrgUnit : OuManagerResolutionActivityBase
{
    [Input(
        DisplayName = "Org Unit Ref ID",
        Description = "The ReferenceId of the starting Organization Unit.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> OrgUnitRefId { get; set; } = default!;

    [Input(
        DisplayName = "Start From Parent",
        Description = "If true, ignores the manager of the starting OU itself and starts searching from its parent OU.",
        DefaultValue = false
    )]
    public Input<bool> StartFromParent { get; set; } = new(false);

    [Input(
        DisplayName = "Include Inactive OU",
        Description = "If true, considers manager positions on inactive OUs.",
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
        string orgUnitRefId = WorkflowJsonHelper.RequireNonEmpty(OrgUnitRefId.GetOrDefault(context), nameof(OrgUnitRefId));
        bool startFromParent = StartFromParent.GetOrDefault(context);
        bool includeInactiveOu = IncludeInactiveOu.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        OuManagerResolutionDto? result = await service.ResolveOuManagerFromOrgUnitAsync(
            orgUnitRefId,
            startFromParent,
            includeInactiveOu,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
