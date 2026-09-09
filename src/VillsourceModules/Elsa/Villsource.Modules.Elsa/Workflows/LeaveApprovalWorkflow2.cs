using Elsa.Expressions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Activities;
using Villsource.Modules.Elsa.Activities;
using Elsa.Scheduling.Activities;

namespace Villsource.Modules.Elsa.Workflows;

public class LeaveApprovalWorkflow2 : WorkflowBase
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

        // ─── 1. NODE DEFINITIONS ───────────────────────────────────────────────

        var calcDays = new Inline(context =>
        {
            var leaveDates = GetWfInput<DateTime[]>(context, "DayToLeave");
            var submitted = GetWfInput<DateTime>(context, "SubmittedDate");

            if (leaveDates != null && leaveDates.Length > 0)
            {
                var startDate = leaveDates.Min();
                daysInAdvance.Set(context, (startDate - submitted).TotalDays);
            }

            return ValueTask.CompletedTask;
        });

        var resolveTier = new ResolveRequesterTier
        {
            EmployeeId = new(context => GetWfInput<string>(context, "RequesterId") ?? ""),
            Result = new(requesterTier)
        };

        var checkAutoReject = new FlowDecision { Condition = new(context => daysInAdvance.Get(context) < 1) };

        var rejectStatus = new UpdateDocumentStatus { DocNo = new(GetDocNo), Status = new("Rejected") };

        var checkAutoApprove = new FlowDecision
        {
            Condition = new(context =>
                IsHighTier(requesterTier.Get(context) ?? "") && daysInAdvance.Get(context) >= 7)
        };

        var approveStatus = new UpdateDocumentStatus { DocNo = new(GetDocNo), Status = new("Approved") };

        var setManager = new SetVariable<string>
        {
            Variable = currentManagerId, Value = new(context => GetWfInput<string>(context, "RequesterId") ?? "")
        };

        var resolveManager = new ResolveManager
        {
            EmployeeId = new(context => currentManagerId.Get(context)!), Result = new(currentManagerId)
        };

        var checkManagerNull = new FlowDecision
        {
            Condition = new(context => string.IsNullOrEmpty(currentManagerId.Get(context)))
        };

        var hrEscalation = new NotifyHrEscalation { DocNo = new(GetDocNo) };

        var sendInbox = new SendInboxTask
        {
            ManagerId = new(context => currentManagerId.Get(context)!), DocNo = new(GetDocNo)
        };

        var managerFork = BuildRaceConditionActivity(daysInAdvance, decision, isFinished);

        var checkIsFinished = new FlowDecision { Condition = new(context => isFinished.Get(context)) };

        var finishEvent = new PublishFinishStateEvent
        {
            ObjectId = new(context => GetDocNo(context)), Result = new(context => decision.Get(context) ?? "!!!")
        };

        // ─── 2. FLOWCHART & CONNECTIONS ────────────────────────────────────────

        builder.Root = new Flowchart
        {
            Activities =
            {
                calcDays,
                resolveTier,
                checkAutoReject,
                rejectStatus,
                checkAutoApprove,
                approveStatus,
                setManager,
                resolveManager,
                checkManagerNull,
                hrEscalation,
                sendInbox,
                managerFork,
                checkIsFinished,
                finishEvent
            },
            Connections =
            {
                // Sequence Initialization (ใช้ Default Ports: Done -> In)
                new Connection(calcDays, resolveTier),
                new Connection(resolveTier, checkAutoReject),

                // Branch 1: Auto Reject
                new Connection(new Endpoint(checkAutoReject, "True"), new Endpoint(rejectStatus, "In")),
                new Connection(rejectStatus, finishEvent),

                // Branch 2: Auto Approve Check
                new Connection(new Endpoint(checkAutoReject, "False"), new Endpoint(checkAutoApprove, "In")),
                new Connection(new Endpoint(checkAutoApprove, "True"), new Endpoint(approveStatus, "In")),
                new Connection(approveStatus, finishEvent),

                // Branch 3: Manual Approval Loop setup
                new Connection(new Endpoint(checkAutoApprove, "False"), new Endpoint(setManager, "In")),
                new Connection(setManager, resolveManager),

                // Manager Resolution
                new Connection(resolveManager, checkManagerNull),

                // Manager Not Found -> Escalate
                new Connection(new Endpoint(checkManagerNull, "True"), new Endpoint(hrEscalation, "In")),
                new Connection(hrEscalation, finishEvent),

                // Manager Found -> Send Task & Wait (Fork)
                new Connection(new Endpoint(checkManagerNull, "False"), new Endpoint(sendInbox, "In")),
                new Connection(sendInbox, managerFork),

                // Post-Fork Check 
                new Connection(managerFork, checkIsFinished),

                // Complete or Loop Back 
                new Connection(new Endpoint(checkIsFinished, "True"), new Endpoint(finishEvent, "In")),
                new Connection(new Endpoint(checkIsFinished, "False"), new Endpoint(resolveManager, "In"))
            }
        };
    }

    private static readonly string[] stringArray = new string[] { "approve", "reject" };

    // ─── HELPER METHODS (Business Logic & Workflow Configuration) ─────────

    public static IActivity BuildRaceConditionActivity(Variable<double> daysInAdvance, Variable<string> decision,
        Variable<bool> isFinished)
    {
        return new Fork((string?)null)
        {
            JoinMode = ForkJoinMode.WaitAny,
            Branches =
            {
                // 🏃 ลู่ที่ 1: รอ Manager ตัดสินใจ (Approve/Reject)
                new Sequence((string?)null)
                {
                    Activities =
                    {
                        new SendToSpecificEmployeeToApprove()
                        {
                            ObjectId = new(context => GetDocNo(context)),
                            RequesterId = new(context => GetWfInput<string>(context, "RequesterId") ?? ""),
                            ReviewerId = new(context => "[TO RESOLVING]"),
                            Options = new(context => stringArray),
                        },
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