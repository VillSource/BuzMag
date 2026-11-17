using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class ActivateCostCenterCommandHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<ActivateCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        ActivateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .Include(c => c.ParentCostCenter)
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.Activate();

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}
