using FluentValidation;
using Mediator;
using Villsource.Modules.Elsa.Contracts.Services;
using Villsource.Modules.Elsa.Contracts.v1.Approval;

namespace Villsource.Modules.Elsa.Feature.v1.Approvals;

public class SubmitToApprovalCommandValidator : AbstractValidator<SubmitToApprovalCommand>
{
    public SubmitToApprovalCommandValidator()
    {
        RuleFor(x => x.ObjectId).NotEmpty().WithMessage("Please provide Object Id");
        RuleFor(x => x.ObjectName).NotEmpty().WithMessage("Please provide Name");
        RuleFor(x => x.FlowDefinitionId).NotEmpty().WithMessage("Please provide Workflow DefinitionId");
        RuleFor(x => x.RequesterId).NotEmpty().WithMessage("Please provide Requester Id");
        RuleFor(x => x.RequestedOn).NotEmpty().WithMessage("Please provide Request On");
    }
}
public class SubmitToApprovalCommandHandler(IApprovalGateway gateway)
    : ICommandHandler<SubmitToApprovalCommand, string>
{
    public async ValueTask<string> Handle(SubmitToApprovalCommand command, CancellationToken cancellationToken)
    {
        var res = await gateway.StartNewWorkflowAsync(command,cancellationToken);

        return res.InstantId ?? "";
    }
}