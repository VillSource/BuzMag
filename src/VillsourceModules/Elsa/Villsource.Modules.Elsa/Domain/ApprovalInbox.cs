using FSH.Framework.Core.Domain;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.Elsa.Domain;

public partial class ApprovalInbox : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public string ObjectId { get; private set; } = null!;
    public string RequesterId { get; private set; } = null!;
    public string ReviewerId { get; private set; } = null!;
    public string WorkflowInstanceId { get; private set; } = null!;
    public string WorkflowDefinitionId { get; private set; } = null!;
    public string[] AllowedActions { get; private set; } = null!;
    public string? RepliedAction { get; private set; }
    public string? Comment { get; private set; }
    public bool IsFinished { get; private set; }

    private ApprovalInbox() { }

    public static ApprovalInbox Create(string objectId, string requesterId, string reviewerId,
        string workflowInstanceId, string workflowDefinitionId, string[] allowedActions)
    {
        return new ApprovalInbox
        {
            ObjectId = objectId,
            RequesterId = requesterId,
            ReviewerId = reviewerId,
            WorkflowInstanceId = workflowInstanceId,
            WorkflowDefinitionId = workflowDefinitionId,
            AllowedActions = allowedActions
        };
    }

    public void MarkAsFinish() => IsFinished = true;

    public void Reply(string action, string? comment)
    {
        RepliedAction = action;
        Comment = comment;
    }
}