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

        // Single DB round-trip: load tracked Position + check direct report, active delegation, OU manager, and dotted line refs
        var target = await dbContext.Positions
                         .Where(p => p.ReferenceId == request.RefId)
                         .Select(p => new
                         {
                             Entity = p,
                             HasDirectReports = dbContext.Positions.Any(r => r.ReportsToPositionId == p.Id && !r.IsDeleted),
                             HasActiveDelegations = dbContext.PositionDelegations.Any(d =>
                                 !d.IsDeleted &&
                                 !d.IsRevoked &&
                                 (d.DelegatorPositionId == p.Id || d.DelegatePositionId == p.Id)),
                             IsOuManager = dbContext.OrganizationUnits.Any(ou => !ou.IsDeleted && ou.ManagerPositionId == p.Id),
                             IsDottedLineTarget = dbContext.Positions.Any(other => !other.IsDeleted && other.DottedLinePositionId == p.Id)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
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

        if (target.IsOuManager)
        {
            throw new InvalidOperationException(
                "Cannot delete a position while it is assigned as the manager of an organization unit.");
        }

        if (target.IsDottedLineTarget)
        {
            throw new InvalidOperationException(
                "Cannot delete a position while positions have a dotted-line reporting relationship to it.");
        }

        target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return target.Entity.ReferenceId;
    }
}