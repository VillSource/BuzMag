using Villsource.Modules.Elsa.Contracts.Dtos;
using Villsource.Modules.Elsa.Contracts.v1.Approval;

namespace Villsource.Modules.Elsa.Contracts.Services;

public interface IApprovalGateway
{
    Task<StartWorkflowResult> StartWorkflowAsync(SubmitToApprovalCommand data,  CancellationToken cancellationToken = default);
}