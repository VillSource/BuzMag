using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class DeleteJobCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<DeleteJobCommand, string>
{
    public async ValueTask<string> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Single DB round-trip: fetch tracked Job + check active Position references
        var target = await dbContext.Jobs
                         .Where(j => j.ReferenceId == request.RefId)
                         .Select(j => new
                         {
                             Entity = j,
                             HasPositions = dbContext.Positions.Any(p => p.JobId == j.Id)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

        if (target.HasPositions)
        {
            throw new InvalidOperationException(
                $"Cannot delete job '{request.RefId}' while positions are assigned to it.");
        }

        target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
        await dbContext.SaveChangesAsync(cancellationToken);

        return target.Entity.ReferenceId;
    }
}