using Elsa.Workflows;

namespace Villsource.Modules.Elsa.Core.Workflows;


public record BuzMagWorkflowBaseInput(
    string ObjectId,
    string ObjectName,
    string RequesterId,         
    DateTimeOffset RequestedOn
);
public abstract class BuzMagWorkflowBase : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.Build(builder);

        builder.WithInput<string>(nameof(BuzMagWorkflowBaseInput.ObjectId));
        builder.WithInput<string>(nameof(BuzMagWorkflowBaseInput.ObjectName));
        builder.WithInput<string>(nameof(BuzMagWorkflowBaseInput.RequesterId));
        builder.WithInput<DateTimeOffset>(nameof(BuzMagWorkflowBaseInput.RequestedOn));
    }
}

public abstract class BuzMagWorkflowBase<T> : WorkflowBase<T>
{
}