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
    DisplayName = "Resolve Active Delegators",
    Description = "Resolves all active delegator (principal) positions whose authority is currently held by a delegate position (returns IReadOnlyList<ResolvedDelegationDto>)."
)]
public class ResolveActiveDelegators : DelegationListResolutionActivityBase
{
    [Input(
        DisplayName = "Delegate Position Ref ID",
        Description = "The ReferenceId of the acting or substitute position receiving delegated authority.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> DelegatePositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Document Type Code",
        Description = "Optional document type code to filter document-scoped delegations.",
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

    [Output(Description = "The highest-priority delegator position (ChainOfCommandPositionDto?), or null when no active delegation exists.")]
    public Output<ChainOfCommandPositionDto?> FirstDelegatorPosition { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string delegatePositionRefId = WorkflowJsonHelper.RequireNonEmpty(DelegatePositionRefId.GetOrDefault(context), nameof(DelegatePositionRefId));
        string? documentTypeCode = WorkflowJsonHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        string? delegationTypeKey = WorkflowJsonHelper.NormalizeOptional(DelegationTypeKey.GetOrDefault(context));
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        IReadOnlyList<ResolvedDelegationDto> result = await service.ResolveActiveDelegatorsAsync(
            delegatePositionRefId,
            documentTypeCode,
            delegationTypeKey,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
        FirstDelegatorPosition.Set(context, result.Count > 0 ? result[0].DelegatorPosition : null);
    }
}
