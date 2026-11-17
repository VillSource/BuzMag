using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Commands;

public sealed class DeactivateJobFamilyCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeactivateJobFamilyCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        DeactivateJobFamilyCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.JobFamilies
            .FirstOrDefaultAsync(f => f.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Job family '{request.RefId}' was not found.");

        entity.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
