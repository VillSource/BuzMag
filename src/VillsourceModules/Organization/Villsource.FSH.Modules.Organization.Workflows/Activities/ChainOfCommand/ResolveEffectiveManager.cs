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
    DisplayName = "Resolve Effective Manager",
    Description = "Resolves the single effective manager/approver for a position with optional OU fallback and delegation substitution (returns EffectiveManagerResolutionDto?)."
)]
public class ResolveEffectiveManager : EffectiveManagerResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the requester or subordinate position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Document Type Code",
        Description = "Optional document type code (e.g., PR, LEAVE) used to match document-scoped delegations.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string?> DocumentTypeCode { get; set; } = default!;

    [Input(
        DisplayName = "Fallback To OU Manager",
        Description = "If true, falls back to crawling up the OU hierarchy when no active direct reporting manager exists.",
        DefaultValue = true
    )]
    public Input<bool> FallbackToOuManager { get; set; } = new(true);

    [Input(
        DisplayName = "Climb To First Active",
        Description = "If true, climbs past inactive positions in the direct reporting line.",
        DefaultValue = true
    )]
    public Input<bool> ClimbToFirstActive { get; set; } = new(true);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp used for validity and delegation checks.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = WorkflowJsonHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        string? documentTypeCode = WorkflowJsonHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        bool fallbackToOuManager = FallbackToOuManager.GetOrDefault(context, static () => true);
        bool climbToFirstActive = ClimbToFirstActive.GetOrDefault(context, static () => true);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        EffectiveManagerResolutionDto? result = await service.ResolveEffectiveManagerAsync(
            positionRefId,
            documentTypeCode,
            fallbackToOuManager,
            climbToFirstActive,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
