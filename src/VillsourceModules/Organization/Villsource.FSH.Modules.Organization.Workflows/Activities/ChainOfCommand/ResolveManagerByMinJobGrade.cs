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
    DisplayName = "Resolve Manager By Min Job Grade",
    Description = "Finds the first active manager in the chain of command meeting the minimum job grade rank order and/or job level category."
)]
public class ResolveManagerByMinJobGrade : EffectiveManagerResolutionActivityBase
{
    [Input(
        DisplayName = "Position Ref ID",
        Description = "The ReferenceId of the starting position.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<string> PositionRefId { get; set; } = default!;

    [Input(
        DisplayName = "Min Rank Order",
        Description = "Optional minimum JobGrade.RankOrder required.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<int?> MinRankOrder { get; set; } = default!;

    [Input(
        DisplayName = "Min Job Level Category",
        Description = "Optional minimum JobLevelCategory key required.",
        UIHint = InputUIHints.DropDown,
        UIHandler = typeof(JobLevelCategoryOptionsProvider),
        DefaultValue = ""
    )]
    public Input<string?> MinJobLevelCategory { get; set; } = default!;

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
        string positionRefId = WorkflowJsonHelper.RequireNonEmpty(PositionRefId.GetOrDefault(context), nameof(PositionRefId));
        int? minRankOrder = MinRankOrder.GetOrDefault(context);
        string? minJobLevelCategory = WorkflowJsonHelper.NormalizeOptional(MinJobLevelCategory.GetOrDefault(context));
        string? documentTypeCode = WorkflowJsonHelper.NormalizeOptional(DocumentTypeCode.GetOrDefault(context));
        bool fallbackToOuManager = FallbackToOuManager.GetOrDefault(context, static () => true);
        DateTimeOffset? asOfUtc = AsOfUtc.GetOrDefault(context);

        EffectiveManagerResolutionDto? result = await service.ResolveManagerByMinJobGradeAsync(
            positionRefId,
            minRankOrder,
            minJobLevelCategory,
            documentTypeCode,
            fallbackToOuManager,
            asOfUtc,
            context.CancellationToken).ConfigureAwait(false);

        SetOutputs(context, result);
    }
}
