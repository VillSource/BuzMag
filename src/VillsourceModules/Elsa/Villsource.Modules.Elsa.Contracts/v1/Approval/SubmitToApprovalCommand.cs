using Mediator;

namespace Villsource.Modules.Elsa.Contracts.v1.Approval;

public record SubmitToApprovalCommand(
    string ObjectId,              
    string FlowDefinitionId,       
    string RequesterId,         
    DateTimeOffset RequestedOn,
    Dictionary<string, object> ContextData ,
    string Resource =  ""
) : ICommand<string>;