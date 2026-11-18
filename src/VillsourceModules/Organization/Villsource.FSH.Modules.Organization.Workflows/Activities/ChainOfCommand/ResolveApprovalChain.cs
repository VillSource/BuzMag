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
    DisplayName = "Resolve Approval Chain",
    Description = "Resolves the multi-step upward approval chain for a position with automatic delegation evaluation."
)]
public class ResolveApprovalChain : JsonListActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the target or requester position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Document Type Code",
        Description = "Optional document type code used to evaluate document-scoped delegations at each step.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string?> DocumentTypeCode { get; set; } = default!;

    [Input(
        DisplayName = "Max Depth",
        Description = "Maximum number of approval steps to resolve.",
        DefaultValue = 15,
        UIHint = InputUIHints.SingleLine
    )]
    public Input<int> MaxDepth { get; set; } = new(15);

    [Input(
        DisplayName = "Fallback To OU Manager",
        Description = "If true, falls back to crawling up the OU hierarchy when no active direct reporting chain exists.",
        DefaultValue = true
    )]
    public Input<bool> FallbackToOuManager { get; set; } = new(true);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used to evaluate position and delegation validity.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    [Output(Description = "First step in the approval chain as a JsonObject, or null when empty.")]
    public Output<JsonObject?> FirstStep { get; set; } = default!;

    [Output(Description = "Effective approver position for the first step as a JsonObject, or null when empty.")]
    public Output<JsonObject?> FirstApproverPosition { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = WorkflowJsonHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        string? documentTypeCode = WorkflowJsonHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        int maxDepth = MaxDepth.GetOrDefault(context, static () => 15);
        bool fallbackToOuManager = FallbackToOuManager.GetOrDefault(context, static () => true);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<EffectiveManagerResolutionDto> result = await service.ResolveApprovalChainAsync(
            positionRefId,
            documentTypeCode,
            maxDepth,
            fallbackToOuManager,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetJsonListResult(context, result);
        FirstStep.Set(context, WorkflowJsonHelper.ToJsonObject(result.Count > 0 ? result[0] : null));
        FirstApproverPosition.Set(context, WorkflowJsonHelper.ToJsonObject(result.Count > 0 ? result[0].EffectiveApproverPosition : null));
    }
}
