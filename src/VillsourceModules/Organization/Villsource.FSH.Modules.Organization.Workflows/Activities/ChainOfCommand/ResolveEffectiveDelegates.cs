using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand.Base;
using Villsource.FSH.Modules.Organization.Workflows.Activities.OptionsProviders;
using Villsource.FSH.Modules.Organization.Workflows.Constants;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

[Activity(
    Category = OrganizationActivityConstants.Category,
    Version = 1,
    DisplayName = "Resolve Effective Delegates",
    Description = "Resolves all currently effective delegate positions granted authority on behalf of a delegator position (returns IReadOnlyList<ResolvedDelegationDto>)."
)]
public class ResolveEffectiveDelegates : DelegationListResolutionActivityBase
{
    [Input(
        DisplayName = "Delegator Position Ref ID",
        Description = "The ReferenceId of the position that delegated its authority.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> DelegatorPositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Document Type Code",
        Description = "Optional document type code (e.g., PR, LEAVE) to evaluate document-scoped delegations.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string?> DocumentTypeCode { get; set; } = default!;

    [Input(
        DisplayName = "Delegation Type",
        Description = "Optional delegation type filter.",
        UIHint = InputUIHints.DropDown,
        UIHandler = typeof(DelegationTypeOptionsProvider),
        DefaultValue = ""
    )]
    public Input<string?> DelegationTypeKey { get; set; } = default!;

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp at which to check delegation effectiveness.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    [Output(Description = "The highest-priority delegate position (ChainOfCommandPositionDto?), or null when no effective delegation exists.")]
    public Output<ChainOfCommandPositionDto?> FirstDelegatePosition { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string delegatorPositionRefId = WorkflowJsonHelper.RequireNonEmpty(DelegatorPositionRefId.GetOrDefault(context), nameof(DelegatorPositionRefId));
        string? documentTypeCode = WorkflowJsonHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        string? delegationTypeKey = WorkflowJsonHelper.NormalizeOptional(DelegationTypeKey.GetOrDefault(context));
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<ResolvedDelegationDto> result = await service.ResolveEffectiveDelegatesAsync(
            delegatorPositionRefId,
            documentTypeCode,
            delegationTypeKey,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
        FirstDelegatePosition.Set(context, result.Count > 0 ? result[0].DelegatePosition : null);
    }
}
