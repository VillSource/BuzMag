using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class DeleteJobGradeCommandHandler(OrganizationDbContext dbContext, ICurrentUser currentUser)
        : IRequestHandler<DeleteJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(DeleteJobGradeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Single DB round-trip to fetch tracked entity + check active references
            var target = await dbContext.JobGrades
                .Where(g => g.ReferenceId == request.RefId)
                .Select(g => new
                {
                    Entity = g,
                    HasJobs = dbContext.Jobs.Any(j => j.DefaultJobGradeId == g.Id),
                    HasPositions = dbContext.Positions.Any(p => p.JobGradeId == g.Id)
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            if (target.HasJobs || target.HasPositions)
            {
                throw new InvalidOperationException(
                    $"Cannot delete job grade '{request.RefId}' while it is referenced by existing jobs or positions.");
            }

            target.Entity.Delete(deletedBy: currentUser.GetUserId().ToString());
            await dbContext.SaveChangesAsync(cancellationToken);

            return target.Entity.ReferenceId;
        }
    }

}