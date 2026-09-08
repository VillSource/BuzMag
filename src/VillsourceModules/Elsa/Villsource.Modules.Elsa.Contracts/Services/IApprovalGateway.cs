using Villsource.Modules.Elsa.Contracts.v1.Approval;

namespace Villsource.Modules.Elsa.Contracts.Services;

public interface IApprovalGateway
{
    Task<string> StartWorkflowAsync(SubmitToApprovalCommand data,  CancellationToken cancellationToken = default);
}