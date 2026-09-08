using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Activities; // 💡 ใช้ Event Activity จาก namespace นี้

namespace Villsource.Modules.Elsa.Workflows;

public class DocumentApprovalWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Name = "DocumentApprovalWorkflow";

        // สร้างตัวแปรมารับค่า Payload ตอนที่ Workflow ถูกปลุก
        var decisionVar = builder.WithVariable<object>();

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine(context => 
                    $"⏳ Request [{context.GetInput<string>("ReqId")}] from {context.GetInput<string>("Name")} is waiting for decision..."),
                
                // 💡 จุดหยุดพัก: รอ Event ชื่อ "WaitDecisionEvent"
                new Event("WaitDecisionEvent")
                {
                    Result = new Output<object?>(decisionVar) // นำ Payload ที่ได้รับใส่ใน decisionVar
                },

                // ทำงานต่อทันทีเมื่อได้รับอนุมัติ/ปฏิเสธ
                new WriteLine(context => 
                    $"✅ Request [{context.GetInput<string>("ReqId")}] was: {decisionVar.Get(context)}")
            }
        };
    }
}