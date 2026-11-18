using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

/// <summary>
/// Extracts fields from a <see cref="ChainOfCommandPositionDto"/> and exposes each field as an output.
/// </summary>
[Activity(
    Category = OrganizationActivityConstants.Category,
    DisplayName = "Chain Of Command Position Field Selector",
    Description = "Extracts fields from a resolved ChainOfCommandPositionDto and outputs each field."
)]
public class ChainOfCommandPositionFieldSelector : CodeActivity<IDictionary<string, object?>>
{
    [Input(
        DisplayName = "Position",
        Description = "The ChainOfCommandPositionDto value output by a resolver activity.",
        UIHint = InputUIHints.VariablePicker
    )]
    public Input<ChainOfCommandPositionDto?> Position { get; set; } = default!;

    [Output(Description = "Position ReferenceId.")]
    public Output<string?> RefId { get; set; } = default!;

    [Output(Description = "Normalized position code.")]
    public Output<string?> Code { get; set; } = default!;

    [Output(Description = "Primary position title.")]
    public Output<string?> Title { get; set; } = default!;

    [Output(Description = "International (English) position title.")]
    public Output<string?> InternationalTitle { get; set; } = default!;

    [Output(Description = "Owning legal entity ReferenceId.")]
    public Output<string?> LegalEntityRefId { get; set; } = default!;

    [Output(Description = "Organization Unit ReferenceId.")]
    public Output<string?> OrgUnitRefId { get; set; } = default!;

    [Output(Description = "Organization Unit code.")]
    public Output<string?> OrgUnitCode { get; set; } = default!;

    [Output(Description = "Organization Unit name.")]
    public Output<string?> OrgUnitName { get; set; } = default!;

    [Output(Description = "Associated Job template code.")]
    public Output<string?> JobCode { get; set; } = default!;

    [Output(Description = "Associated JobGrade code.")]
    public Output<string?> JobGradeCode { get; set; } = default!;

    [Output(Description = "JobGrade numeric rank order.")]
    public Output<int?> JobGradeRankOrder { get; set; } = default!;

    [Output(Description = "Standardized job level category key.")]
    public Output<string?> JobLevelCategory { get; set; } = default!;

    [Output(Description = "Maximum financial approval limit amount.")]
    public Output<decimal?> ApprovalLimitAmount { get; set; } = default!;

    [Output(Description = "ISO 3-letter currency code for the approval limit.")]
    public Output<string?> ApprovalLimitCurrency { get; set; } = default!;

    [Output(Description = "Whether this position is flagged as primary leadership.")]
    public Output<bool?> IsPrimaryLeadership { get; set; } = default!;

    [Output(Description = "Whether this position is currently active.")]
    public Output<bool?> IsActive { get; set; } = default!;

    [Output(Description = "UTC start timestamp of the position validity window.")]
    public Output<DateTimeOffset?> ValidFrom { get; set; } = default!;

    [Output(Description = "UTC end timestamp of the position validity window.")]
    public Output<DateTimeOffset?> ValidTo { get; set; } = default!;

    protected override void Execute(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ChainOfCommandPositionDto? position = Position.GetOrDefault(context);
        IReadOnlyList<PositionFieldDefinition> definitions = ChainOfCommandPositionFields.All;
        Dictionary<string, object?> fieldValues = new(definitions.Count, StringComparer.Ordinal);

        foreach (PositionFieldDefinition definition in definitions)
        {
            object? value = position is null ? null : definition.Accessor(position);
            fieldValues[definition.Name] = value;
            definition.OutputSetter(this, context, value, definition.Name);
        }

        context.SetResult(fieldValues);
    }
}
