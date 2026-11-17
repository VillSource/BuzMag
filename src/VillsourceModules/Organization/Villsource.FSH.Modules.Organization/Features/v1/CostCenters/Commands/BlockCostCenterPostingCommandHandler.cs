using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class BlockCostCenterPostingCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<BlockCostCenterPostingCommand, string>
{
    public async ValueTask<string> Handle(
        BlockCostCenterPostingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.BlockPosting();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}
