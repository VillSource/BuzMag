using Elsa.Workflows.Runtime;
using Mediator;

namespace Villsource.Modules.Elsa.Feature.v1;

public record ApproveRequestCommand(string ReqId, string Decision) : IRequest<string>;

// 💡 Inject IEventPublisher 
public class ApproveRequestHandler(IEventPublisher eventPublisher) : IRequestHandler<ApproveRequestCommand, string>
{
    public async ValueTask<string> Handle(ApproveRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        // 💡 ยิงสัญญาณเข้าสู่ระบบของ Elsa
        await eventPublisher.PublishAsync(
            eventName: "WaitDecisionEvent",
            correlationId: command.ReqId,       // จับคู่กับ Workflow ที่มี ReqId ตรงกัน
            payload: command.Decision,          // ส่งคำว่า "Approve" หรือ "Reject" เข้าไป
            cancellationToken: cancellationToken);

        return $"Sent '{command.Decision}' to ReqId: {command.ReqId}";
    }
}