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
    DisplayName = "Resolve Manager By Approval Limit",
    Description = "Finds the first active manager in the chain of command whose approval limit meets the required amount and currency."
)]
public class ResolveManagerByApprovalLimit : EffectiveManagerResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the starting position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Required Amount",
        Description = "Minimum approval limit amount required (must be non-negative).",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<decimal> RequiredAmount { get; set; } = default!;

    [Input(
        DisplayName = "Currency",
        Description = "3-letter ISO currency code (e.g., THB, USD).",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> Currency { get; set; } = default!;

    [Input(
        DisplayName = "Document Type Code",
        Description = "Optional document type code used to resolve active delegations for the qualifying manager.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string?> DocumentTypeCode { get; set; } = default!;

    [Input(
        DisplayName = "Fallback To OU Manager",
        Description = "If true, also searches up the OU leadership hierarchy if the direct reporting chain yields no qualifying manager.",
        DefaultValue = true
    )]
    public Input<bool> FallbackToOuManager { get; set; } = new(true);

    [Input(
        DisplayName = "As Of (UTC)",
        Description = "Optional UTC timestamp for validity and delegation evaluation.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<DateTimeOffset?> AsOfUtc { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IChainOfCommandService service = context.GetRequiredService<IChainOfCommandService>();
        string positionRefId = ChainOfCommandActivityHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        decimal requiredAmount = RequiredAmount.Get(context);
        string currency = ChainOfCommandActivityHelper.RequireNonEmpty(Currency.GetOrDefault(context), nameof(Currency));
        string? documentTypeCode = ChainOfCommandActivityHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        bool fallbackToOuManager = FallbackToOuManager.GetOrDefault(context, static () => true);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        EffectiveManagerResolutionDto? result = await service.ResolveManagerByApprovalLimitAsync(
            positionRefId,
            requiredAmount,
            currency,
            documentTypeCode,
            fallbackToOuManager,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
