using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;

using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.Elsa.Core.Activities.OptionsProviders;
using Villsource.Modules.HumanResource.Workflow.Activities.OptionsProviders;
using Villsource.Modules.HumanResource.Workflow.Contract.Constants;

namespace Villsource.Modules.HumanResource.Workflow.Activities;

[Activity(
    Category = HrActivityConstant.Category,
    Description =
        "Evaluates an input Position Tier against a target tier and routes the workflow to True or False branches.",
    DisplayName = "Evaluate Position Tier"
)]
[FlowNode("True", "False")]
public class EvaluatePositionTier : Activity
{
    [Input(
        Description = "The position tier to evaluate (Bind this to your Workflow Input or Variable).",
        UIHint = InputUIHints.VariablePicker
    )]
    public Input<string> InputTier { get; set; } = null!;

    [Input(
        Description = "Select the comparison operator.",
        UIHint = InputUIHints.DropDown,
        UIHandler = typeof(ComparisonOperatorOptionsProvider)
    )]
    public Input<string> Operator { get; set; } = default!;

    [Input(
        Description = "Select the target Position Tier to compare against.",
        UIHint = InputUIHints.DropDown,
        UIHandler = typeof(PositionTierOptionsProvider)
    )]
    public Input<string> TargetTier { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // ดึงค่า
        var inputTierStr = InputTier.Get(context);
        var op = Operator.Get(context);
        var targetTierStr = TargetTier.Get(context);

        // ถ้าข้อมูลไม่ครบ วิ่งไปเส้น False ทันที
        if (string.IsNullOrWhiteSpace(inputTierStr) ||
            string.IsNullOrWhiteSpace(op) ||
            string.IsNullOrWhiteSpace(targetTierStr))
        {
            await context.CompleteActivityWithOutcomesAsync("False");
            return;
        }

        // แปลง String เป็น PositionTier
        if (!PositionTier.TryGet(inputTierStr, out var inputTier) ||
            !PositionTier.TryGet(targetTierStr, out var targetTier))
        {
            await context.CompleteActivityWithOutcomesAsync("False");
            return;
        }

        int inputVal = inputTier.Tier;
        int targetVal = targetTier.Tier;

        // คำนวณเงื่อนไข
        bool isMatch = op switch
        {
            "==" => inputVal == targetVal,
            "!=" => inputVal != targetVal,
            ">" => inputVal > targetVal,
            "<" => inputVal < targetVal,
            ">=" => inputVal >= targetVal,
            "<=" => inputVal <= targetVal,
            _ => false
        };

        // 4. สั่งจบการทำงานของ Activity พร้อมกับระบุว่าให้ Workflow วิ่งไปเส้นไหน
        var outcome = isMatch ? "True" : "False";
        await context.CompleteActivityWithOutcomesAsync(outcome);
    }
}