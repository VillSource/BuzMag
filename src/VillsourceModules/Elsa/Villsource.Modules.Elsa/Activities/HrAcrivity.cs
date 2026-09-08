using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using Villsource.Modules.Elsa.Contracts.Services;

namespace Villsource.Modules.Elsa.Activities;

public class ResolveRequesterTier : CodeActivity<string>
{
    public Input<string> EmployeeId { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        var resolver = context.GetRequiredService<IEmployeeResolver>();
        var emp = await resolver.GetEmployeeAsync(EmployeeId.Get(context), context.CancellationToken);
        
        // ส่งชื่อตำแหน่งกลับไปเทียบค่าใน Workflow
        context.SetResult(emp?.PositionTierName ?? "NONE"); 
    }
}

public class ResolveManager : CodeActivity<string?>
{
    public Input<string> EmployeeId { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var resolver = context.GetRequiredService<IEmployeeResolver>();
        var managerId = await resolver.GetPrimaryManagerAsync(EmployeeId.Get(context), context.CancellationToken);
        context.SetResult(managerId);
    }
}

public class SendInboxTask : CodeActivity
{
    public Input<string> ManagerId { get; set; } = default!;
    public Input<string> DocNo { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var notifier = context.GetRequiredService<INotificationService>();
        await notifier.SendApprovalTaskAsync(ManagerId.Get(context), DocNo.Get(context), context.CancellationToken);
    }
}

public class UpdateDocumentStatus : CodeActivity
{
    public Input<string> DocNo { get; set; } = default!;
    public Input<string> Status { get; set; } = default!;
    public Input<bool> IsFinish { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var callback = context.GetRequiredService<IDocumentCallback>();
        await callback.UpdateStatusAsync(DocNo.Get(context), Status.Get(context), IsFinish.Get(context), context.CancellationToken);
    }
}

public class NotifyHrEscalation : CodeActivity
{
    public Input<string> DocNo { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var notifier = context.GetRequiredService<INotificationService>();
        await notifier.NotifyHrEscalationAsync(DocNo.Get(context), "No manager available for approval.", context.CancellationToken);
    }
}
