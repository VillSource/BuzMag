using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class DeleteOrganizationUnitTypeCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeleteOrganizationUnitTypeCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        DeleteOrganizationUnitTypeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnitTypes
            .FirstOrDefaultAsync(t => t.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.RefId}' was not found.");

        if (await dbContext.OrganizationUnits
                .AnyAsync(ou => ou.OrganizationUnitTypeId == entity.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"Organization unit type '{entity.Code}' is in use by one or more organization units.");
        }

        entity.Delete();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
