using Mediator;

namespace Villsource.Modules.Elsa.Contracts.v1.Approval;

public record SubmitDecisionCommand(
    string ObjectId, 
    string InstanceId, 
    string Decision, 
    string ReviewerId
) : ICommand<string>;