using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Commands;

public sealed class DeleteCostCenterCommandHandler(
    OrganizationDbContext dbContext,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteCostCenterCommand, string>
{
    public async ValueTask<string> Handle(
        DeleteCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var costCenter = await dbContext.CostCenters
                             .Include(c => c.Children.Where(child => !child.IsDeleted))
                             .FirstOrDefaultAsync(
                                 predicate: c => c.ReferenceId == request.RefId,
                                 cancellationToken: cancellationToken)
                         ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        costCenter.Delete(deletedBy: currentUser.GetUserId().ToString());

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return costCenter.ReferenceId;
    }
}
