using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Commands
{
    public sealed class ActivateJobGradeCommandHandler(OrganizationDbContext dbContext)
        : IRequestHandler<ActivateJobGradeCommand, string>
    {
        public async ValueTask<string> Handle(ActivateJobGradeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await dbContext.JobGrades
                             .FirstOrDefaultAsync(g => g.ReferenceId == request.RefId, cancellationToken)
                         ?? throw new NotFoundException($"Job grade '{request.RefId}' was not found.");

            if (!entity.IsActive)
            {
                entity.Activate();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity.ReferenceId;
        }
    }
}