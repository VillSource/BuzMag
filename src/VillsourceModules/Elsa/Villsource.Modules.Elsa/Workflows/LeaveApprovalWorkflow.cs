using Elsa.Expressions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Elsa.Extensions; // 💡 สำคัญสำหรับดึง ActivityExecutionContext
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Activities;
using Villsource.Modules.Elsa.Activities;
using System; // 💡 สำหรับแก้ Error 'TimeSpan'
using Elsa.Scheduling.Activities; // 💡 สำหรับแก้ Error 'Delay'

namespace Villsource.Modules.Elsa.Workflows;

public class LeaveApprovalWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Name = "LeaveApprovalWorkflow";

        // Inputs
        builder.WithInput<string>("DocNo");
        builder.WithInput<string>("Resource");
        builder.WithInput<string>("LeaveType");
        builder.WithInput<DateTime[]>("DayToLeave");
        builder.WithInput<string>("RequesterId");
        builder.WithInput<DateTime>("SubmittedDate");
        builder.WithInput<string>("Property");

        // Variables (State)
        var daysInAdvance = builder.WithVariable<double>();
        var requesterTier = builder.WithVariable<string>();
        var currentManagerId = builder.WithVariable<string>();
        var decision = builder.WithVariable<string>();
        var isFinished = builder.WithVariable<bool>(false);

        builder.Root = new Sequence
        {
            Activities =
            {
                // 1. คำนวณวันลาล่วงหน้า
                new Inline(context =>
                {
                    var leaveDates = GetWfInput<DateTime[]>(context, "DayToLeave");
                    var submitted = GetWfInput<DateTime>(context, "SubmittedDate");

                    if (leaveDates != null && leaveDates.Length > 0)
                    {
                        var startDate = leaveDates.Min();
                        daysInAdvance.Set(context, (startDate - submitted).TotalDays);
                    }

                    return ValueTask.CompletedTask;
                }),

                // 2. ดึงข้อมูลผู้ขอลา
                new ResolveRequesterTier
                {
                    EmployeeId = new(context => GetWfInput<string>(context, "RequesterId") ?? ""),
                    Result = new(requesterTier)
                },

                // 3. Auto-Reject (ลาน้อยกว่า 1 วัน)
                new If
                {
                    Condition = new(context => daysInAdvance.Get(context) < 1),
                    Then = new Sequence
                    {
                        Activities =
                        {
                            new UpdateDocumentStatus { DocNo = new(GetDocNo), Status = new("Rejected") },
                            new SetVariable<bool> { Variable = isFinished, Value = new(true) }
                        }
                    }
                },

                // 4. Auto-Approve (Tier สูง และลาล่วงหน้า 7 วันขึ้นไป)
                new If
                {
                    Condition = new(context => !isFinished.Get(context) &&
                                               IsHighTier(requesterTier.Get(context) ?? "") &&
                                               daysInAdvance.Get(context) >= 7),
                    Then = new Sequence
                    {
                        Activities =
                        {
                            new UpdateDocumentStatus { DocNo = new(GetDocNo), Status = new("Approved") },
                            new SetVariable<bool> { Variable = isFinished, Value = new(true) }
                        }
                    }
                },

                // 5. เซ็ต Manager เริ่มต้นและเข้าสู่ลูปอนุมัติ
                new SetVariable<string>
                {
                    Variable = currentManagerId,
                    Value = new(context => GetWfInput<string>(context, "RequesterId") ?? "")
                },
                new While(null as string)
                {
                    Condition = new(context => !isFinished.Get(context)),
                    Body = new Sequence()
                    {
                        Activities =
                        {
                            new ResolveManager
                            {
                                EmployeeId = new(context => currentManagerId.Get(context)!),
                                Result = new(currentManagerId)
                            },
                            new If()
                            {
                                Condition =
                                    new(context => string.IsNullOrEmpty(currentManagerId.Get(context))),
                                Then = new Sequence()
                                {
                                    Activities =
                                    {
                                        new NotifyHrEscalation { DocNo = new(GetDocNo) },
                                        new SetVariable<bool> { Variable = isFinished, Value = new(true) }
                                    }
                                },
                                Else = new Sequence()
                                {
                                    Activities =
                                    {
                                        new SendInboxTask
                                        {
                                            ManagerId = new(context =>
                                                currentManagerId.Get(context)!),
                                            DocNo = new(GetDocNo)
                                        },
                                        BuildRaceConditionActivity(daysInAdvance, decision,
                                            isFinished)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    // ─── HELPER METHODS (Business Logic & Workflow Configuration) ─────────

// 💡 เปลี่ยน Return Type เป็น IActivity เพราะเราสามารถส่ง Fork กลับไปตรงๆ ได้เลย

    public static IActivity BuildRaceConditionActivity(Variable<double> daysInAdvance, Variable<string> decision,
        Variable<bool> isFinished)
    {
        return new Fork((string?)null)
        {
            JoinMode = ForkJoinMode.WaitAny, // 💡 แก้ไขเป็น ForkJoinMode ตรงนี้
            Branches =
            {
                // 🏃 ลู่ที่ 1: รอ Manager ตัดสินใจ (Approve/Reject)
                new Sequence((string?)null)
                {
                    Activities =
                    {
                        new Event("event") { EventName = new("ManagerDecisionEvent"), Result = new(decision) },
                        new UpdateDocumentStatus
                        {
                            DocNo = new((ExpressionExecutionContext context) => GetDocNo(context)),
                            Status = new((ExpressionExecutionContext context) =>
                                decision.Get(context) ?? "Rejected")
                        },
                        new SetVariable<bool>((string?)null) { Variable = isFinished, Value = new(true) }
                    }
                },

                // 🏃 ลู่ที่ 2: รอครบ 5 วัน (Timeout)
                new Sequence((string?)null)
                {
                    Activities =
                    {
                        new Delay((string?)null) { TimeSpan = new(TimeSpan.FromDays(5)) },
                        new WriteLine("5 Days timeout, escalating to next manager...")
                    }
                },

                // 🏃 ลู่ที่ 3: ถึงวันลาจริง (Auto-Cancel)
                new Sequence((string?)null)
                {
                    Activities =
                    {
                        new Delay((string?)null)
                        {
                            TimeSpan = new((ExpressionExecutionContext context) =>
                                TimeSpan.FromDays(daysInAdvance.Get(context)))
                        },
                        new UpdateDocumentStatus
                        {
                            DocNo = new((ExpressionExecutionContext context) => GetDocNo(context)),
                            Status = new("Cancelled")
                        },
                        new SetVariable<bool>((string?)null) { Variable = isFinished, Value = new(true) }
                    }
                }
            }
        };
    }

    private static string GetDocNo(ExpressionExecutionContext context)
        => GetWfInput<string>(context, "DocNo") ?? "";

    private static bool IsHighTier(string tierName)
    {
        string[] highTiers = { "DIRECTOR", "PRINCIPAL", "EXECUTIVE", "DISTINGUISHED" };
        return Array.Exists(highTiers, t => t.Equals(tierName, StringComparison.OrdinalIgnoreCase));
    }

    // ─── SAFE INPUT RETRIEVAL HELPERS ─────────

    private static T? GetWfInput<T>(ActivityExecutionContext context, string key)
    {
        return context.WorkflowExecutionContext.Input.TryGetValue(key, out var val)
            ? (T?)val
            : default;
    }

    private static T? GetWfInput<T>(ExpressionExecutionContext context, string key)
    {
        if (context.TryGetActivityExecutionContext(out var activityContext) &&
            activityContext.WorkflowExecutionContext.Input.TryGetValue(key, out var val))
        {
            return (T?)val;
        }

        return default;
    }
}