using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Constants;
using Villsource.Modules.Elsa.Core.Helpers;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

/// <summary>
/// Parses a <see cref="ChainOfCommandPositionDto"/> (or JSON representation) and exposes each field as a typed output.
/// </summary>
[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Chain Of Command Position Field Selector",
    Description = "Extracts typed fields from a ChainOfCommandPositionDto (or JSON) and outputs each field to variables or expressions (returns ChainOfCommandPositionDto?)."
)]
public class ChainOfCommandPositionFieldSelector : CodeActivity<ChainOfCommandPositionDto?>
{
    [Input(
        DisplayName = "Position",
        Description = "ChainOfCommandPositionDto (or JSON string / object) output by a resolver activity.",
        UIHint = InputUIHints.SingleLine
    )]
    public Input<object?> Position { get; set; } = default!;

    [Output(Description = "Position ReferenceId (string?).")]
    public Output<string?> RefId { get; set; } = default!;

    [Output(Description = "Normalized position code (string?).")]
    public Output<string?> Code { get; set; } = default!;

    [Output(Description = "Primary position title (string?).")]
    public Output<string?> Title { get; set; } = default!;

    [Output(Description = "International (English) position title (string?).")]
    public Output<string?> InternationalTitle { get; set; } = default!;

    [Output(Description = "Owning legal entity ReferenceId (string?).")]
    public Output<string?> LegalEntityRefId { get; set; } = default!;

    [Output(Description = "Organization Unit ReferenceId (string?).")]
    public Output<string?> OrgUnitRefId { get; set; } = default!;

    [Output(Description = "Organization Unit code (string?).")]
    public Output<string?> OrgUnitCode { get; set; } = default!;

    [Output(Description = "Organization Unit name (string?).")]
    public Output<string?> OrgUnitName { get; set; } = default!;

    [Output(Description = "Associated Job template code (string?).")]
    public Output<string?> JobCode { get; set; } = default!;

    [Output(Description = "Associated JobGrade code (string?).")]
    public Output<string?> JobGradeCode { get; set; } = default!;

    [Output(Description = "JobGrade numeric rank order (int?).")]
    public Output<int?> JobGradeRankOrder { get; set; } = default!;

    [Output(Description = "Standardized job level category key (string?).")]
    public Output<string?> JobLevelCategory { get; set; } = default!;

    [Output(Description = "Maximum financial approval limit amount (decimal?).")]
    public Output<decimal?> ApprovalLimitAmount { get; set; } = default!;

    [Output(Description = "ISO 3-letter currency code for the approval limit (string?).")]
    public Output<string?> ApprovalLimitCurrency { get; set; } = default!;

    [Output(Description = "Whether this position is flagged as primary leadership (bool?).")]
    public Output<bool?> IsPrimaryLeadership { get; set; } = default!;

    [Output(Description = "Whether this position is currently active (bool?).")]
    public Output<bool?> IsActive { get; set; } = default!;

    [Output(Description = "UTC start timestamp of the position validity window (DateTimeOffset?).")]
    public Output<DateTimeOffset?> ValidFrom { get; set; } = default!;

    [Output(Description = "UTC end timestamp of the position validity window (DateTimeOffset?).")]
    public Output<DateTimeOffset?> ValidTo { get; set; } = default!;

    protected override void Execute(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ChainOfCommandPositionDto? position = WorkflowJsonHelper.Parse<ChainOfCommandPositionDto>(Position.GetOrDefault(context));
        IReadOnlyList<PositionFieldDefinition> definitions = ChainOfCommandPositionFields.All;

        foreach (PositionFieldDefinition definition in definitions)
        {
            object? value = position is null ? null : definition.Accessor(position);
            definition.OutputSetter(this, context, value, definition.Name);
        }

        context.SetResult(position);
    }
}
