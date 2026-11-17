using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class DeleteJobFamilyCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeleteJobFamilyCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        DeleteJobFamilyCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.JobFamilies
            .FirstOrDefaultAsync(f => f.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Job family '{request.RefId}' was not found.");

        if (await dbContext.Jobs
                .AnyAsync(j => j.JobFamilyId == entity.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Job family '{entity.Code}' is in use by one or more jobs.");
        }

        entity.Delete();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
