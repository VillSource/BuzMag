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

        // Single DB round-trip: load tracked Position + check active direct reports via SQL EXISTS
        var target = await dbContext.Positions
                         .Where(p => p.ReferenceId == request.RefId)
                         .Select(p => new
                         {
                             Entity = p,
                             HasActiveDirectReports = dbContext.Positions.Any(r => r.ReportsToPositionId == p.Id && r.IsActive)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        if (target.HasActiveDirectReports)
        {
            throw new InvalidOperationException(
                "Cannot deactivate a position while active direct report positions still exist.");
        }

        if (target.Entity.IsActive)
        {
            target.Entity.Deactivate();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return target.Entity.ReferenceId;
    }
}