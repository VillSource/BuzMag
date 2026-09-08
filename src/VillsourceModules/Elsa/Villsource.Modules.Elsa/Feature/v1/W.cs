using Elsa.Common.Multitenancy;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using FSH.Framework.Core.Context;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Villsource.Modules.Elsa.Feature.v1;

public record SubmitLeaveCommand(
    string DocNo,
    string Resource,
    string LeaveType,
    DateTime[] DayToLeave,
    string RequesterId,
    DateTime SubmittedDate,
    string Property) : ICommand<string>;

public class SubmitLeaveHandler(
    IWorkflowRuntime workflowRuntime, 
    ICurrentUser user, 
    ITenantAccessor tenantAccessor) : ICommandHandler<SubmitLeaveCommand, string>
{
    public async ValueTask<string> Handle(SubmitLeaveCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var client = await workflowRuntime.CreateClientAsync(cancellationToken);
        
        var tenant = tenantAccessor.Tenant;
        _ = tenant;

        var request = new CreateAndRunWorkflowInstanceRequest
        {
            // กำหนด CorrelationId เพื่อใช้อ้างอิงเอกสารตอนปลุก Workflow (Manager Approve)
            CorrelationId = command.DocNo,
            Input = new Dictionary<string, object>
            {
                ["DocNo"] = command.DocNo,
                ["Resource"] = command.Resource,
                ["LeaveType"] = command.LeaveType,
                ["DayToLeave"] = command.DayToLeave,
                ["RequesterId"] = command.RequesterId,
                ["SubmittedDate"] = command.SubmittedDate,
                ["Property"] = command.Property,
                ["Tenant"] = user.GetTenant() ?? string.Empty
            },
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId("LeaveApprovalWorkflow"),
        };

        var result = await client.CreateAndRunInstanceAsync(request, cancellationToken);
        return $"Cross-Module Execution Success! Instance ID: {result.WorkflowInstanceId}";
    }
}





// 💡 1. Command รับค่าจาก API (ใช้ DocNo และ Decision)
public record ManagerDecisionCommand(string DocNo, string Decision) : ICommand<string>;

// 💡 2. Inject IEventPublisher แบบในตัวอย่าง
public class ManagerDecisionHandler(IEventPublisher eventPublisher) : ICommandHandler<ManagerDecisionCommand, string>
{
    public async ValueTask<string> Handle(ManagerDecisionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        // 💡 ยิงสัญญาณเข้าสู่ระบบของ Elsa ให้ไปปลุก Activity 'Event' ที่รออยู่
        await eventPublisher.PublishAsync(
            eventName: "ManagerDecisionEvent",  // 🔥 ต้องตรงกับชื่อ EventName ใน Workflow
            correlationId: command.DocNo,       // 🔥 จับคู่กับ Workflow Instance ที่มี DocNo ตรงกัน
            payload: command.Decision,          // 💡 ส่ง "Approved" หรือ "Rejected" ไปลงตัวแปร decision
            cancellationToken: cancellationToken);

        return $"Sent '{command.Decision}' to DocNo: {command.DocNo}";
    }
}




public static class LeaveApprovalEndpoints
{
    public static void MapLeaveApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/leaves").WithTags("Leave Approval Workflow");

        group.MapPost("/submit", async (
                SubmitLeaveCommand command, 
                ISender mediator, 
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(new { Message = result });
            })
            .WithName("SubmitLeaveWorkflow")
            .WithSummary("สร้างคำขอลาและเริ่ม Workflow");

        group.MapPost("/decision", async (
                ManagerDecisionCommand command, 
                ISender mediator, 
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(new { Message = result });
            })
            .WithName("TriggerManagerDecision")
            .WithSummary("ผู้จัดการอนุมัติหรือปฏิเสธคำขอลา");
    }
}