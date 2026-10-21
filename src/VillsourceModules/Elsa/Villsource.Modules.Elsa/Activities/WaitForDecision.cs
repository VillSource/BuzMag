using Elsa.Extensions;
using Elsa.Workflows;

namespace Villsource.Modules.Elsa.Activities;

public class WaitForDecision : Activity<string>
{
    protected override void Execute(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.CreateBookmark("WaitDecisionBookmark", OnResumeAsync);
    }

    private static ValueTask OnResumeAsync(ActivityExecutionContext context)
    {
        // 💡 เมื่อถูกปลุก (Resume) จะดึงค่า Decision ที่ส่งเข้ามา
        var decision = context.GetWorkflowInput<string>("Decision") ?? "Reject";
        
        // ตั้งค่าผลลัพธ์ของ Activity แล้วทำงานต่อ
        context.SetResult(decision);
        return ValueTask.CompletedTask;
    }
}