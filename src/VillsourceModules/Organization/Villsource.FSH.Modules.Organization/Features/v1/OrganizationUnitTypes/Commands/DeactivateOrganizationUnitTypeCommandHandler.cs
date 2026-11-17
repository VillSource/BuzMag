using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class DeactivateOrganizationUnitTypeCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeactivateOrganizationUnitTypeCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        DeactivateOrganizationUnitTypeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnitTypes
            .FirstOrDefaultAsync(t => t.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.RefId}' was not found.");

        entity.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
