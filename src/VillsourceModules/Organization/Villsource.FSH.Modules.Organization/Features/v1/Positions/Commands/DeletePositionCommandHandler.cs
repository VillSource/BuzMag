using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class DeletePositionCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<DeletePositionCommand, string>
{
    public async ValueTask<string> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Single DB round-trip: load tracked Position + check direct report or active delegation references
        var target = await dbContext.Positions
                         .Where(p => p.ReferenceId == request.RefId)
                         .Select(p => new
                         {
                             Entity = p,
                             HasDirectReports = dbContext.Positions.Any(r => r.ReportsToPositionId == p.Id),
                             HasActiveDelegations = dbContext.PositionDelegations.Any(d =>
                                 !d.IsRevoked &&
                                 (d.DelegatorPositionId == p.Id || d.DelegatePositionId == p.Id))
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (target.HasDirectReports)
        {
            throw new InvalidOperationException(
                "Cannot delete a position while direct report positions still exist.");
        }

        if (target.HasActiveDelegations)
        {
            throw new InvalidOperationException(
                "Cannot delete a position while active position delegations reference it.");
        }

        target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
        await dbContext.SaveChangesAsync(cancellationToken);

        return target.Entity.ReferenceId;
    }
}