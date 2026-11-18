using System.Text.Json.Nodes;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Constants;
using Villsource.Modules.Elsa.Core.Activities.Base;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Resolve OU Leadership Chain",
    Description = "Resolves the upward Organization Unit leadership chain from a starting OU up to the root OU."
)]
public class ResolveOuLeadershipChain : JsonListActivityBase
{
    [Input(
        DisplayName = "Org Unit Ref ID",
        Description = "The ReferenceId of the starting Organization Unit.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> OrgUnitRefId { get; set; } = default!;

    [Input(
        DisplayName = "Only Units With Manager",
        Description = "If true, filters the returned chain to only OUs that have an active manager position assigned.",
        DefaultValue = false
    )]
    public Input<bool> OnlyUnitsWithManager { get; set; } = new(false);

    [Input(
        DisplayName = "Include Inactive OU",
        Description = "If true, includes inactive ancestor OUs.",
        DefaultValue = false
    )]
    public Input<bool> IncludeInactiveOu { get; set; } = new(false);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate manager position validity windows.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    [Output(Description = "First step in the OU leadership chain as a JsonObject, or null when empty.")]
    public Output<JsonObject?> FirstStep { get; set; } = default!;

    [Output(Description = "Manager position from the first step that has one assigned as a JsonObject, or null if none.")]
    public Output<JsonObject?> FirstManagerPosition { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string orgUnitRefId = WorkflowJsonHelper.RequireNonEmpty(OrgUnitRefId.GetOrDefault(context), nameof(OrgUnitRefId));
        bool onlyUnitsWithManager = OnlyUnitsWithManager.GetOrDefault(context);
        bool includeInactiveOu = IncludeInactiveOu.GetOrDefault(context);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<OuLeadershipStepDto> result = await service.ResolveOuLeadershipChainAsync(
            orgUnitRefId,
            onlyUnitsWithManager,
            includeInactiveOu,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetJsonListResult(context, result);
        FirstStep.Set(context, WorkflowJsonHelper.ToJsonObject(result.Count > 0 ? result[0] : null));
        FirstManagerPosition.Set(context, WorkflowJsonHelper.ToJsonObject(result.Select(static s => s.ManagerPosition).FirstOrDefault(static m => m is not null)));
    }
}
