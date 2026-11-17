using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Commands;

public sealed class DeactivatePositionCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeactivatePositionCommand, string>
{
    public async ValueTask<string> Handle(DeactivatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Single DB round-trip: load tracked Position + check active direct reports, OU manager, and dotted line refs
        var target = await dbContext.Positions
                         .Where(p => p.ReferenceId == request.RefId)
                         .Select(p => new
                         {
                             Entity = p,
                             HasActiveDirectReports = dbContext.Positions.Any(r => r.ReportsToPositionId == p.Id && !r.IsDeleted && r.IsActive),
                             IsOuManager = dbContext.OrganizationUnits.Any(ou => !ou.IsDeleted && ou.IsActive && ou.ManagerPositionId == p.Id),
                             IsDottedLineTarget = dbContext.Positions.Any(other => !other.IsDeleted && other.IsActive && other.DottedLinePositionId == p.Id)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (target.HasActiveDirectReports)
        {
            throw new InvalidOperationException(
                "Cannot deactivate a position while active direct report positions still exist.");
        }

        if (target.IsOuManager)
        {
            throw new InvalidOperationException(
                "Cannot deactivate a position while it is assigned as the manager of an active organization unit.");
        }

        if (target.IsDottedLineTarget)
        {
            throw new InvalidOperationException(
                "Cannot deactivate a position while active positions have a dotted-line reporting relationship to it.");
        }

        if (target.Entity.IsActive)
        {
            target.Entity.Deactivate();
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return target.Entity.ReferenceId;
    }
}