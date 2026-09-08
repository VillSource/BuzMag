using Mediator;

namespace Villsource.Modules.Elsa.Contracts.v1.Approval;

public record SubmitToApprovalCommand(
    string ObjectId,              
    string FlowIdentifier,       
    string RequesterId,         
    DateTimeOffset RequestedOn,
    Dictionary<string, object> ContextData ,
    string? Resource = null
) : ICommand<string>;