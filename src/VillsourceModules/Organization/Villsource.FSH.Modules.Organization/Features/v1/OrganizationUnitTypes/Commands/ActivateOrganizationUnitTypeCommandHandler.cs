using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Commands;

public sealed class ActivateOrganizationUnitTypeCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<ActivateOrganizationUnitTypeCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        ActivateOrganizationUnitTypeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnitTypes
            .FirstOrDefaultAsync(t => t.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.RefId}' was not found.");

        entity.Activate();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
