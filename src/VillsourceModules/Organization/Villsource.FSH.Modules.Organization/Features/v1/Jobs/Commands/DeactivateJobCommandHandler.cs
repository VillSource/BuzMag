using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class DeactivateJobCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeactivateJobCommand, string>
{
    public async ValueTask<string> Handle(DeactivateJobCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.Jobs
                         .FirstOrDefaultAsync(j => j.ReferenceId == request.RefId, cancellationToken)
                     ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

        if (entity.IsActive)
        {
            entity.Deactivate();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return entity.ReferenceId;
    }
}