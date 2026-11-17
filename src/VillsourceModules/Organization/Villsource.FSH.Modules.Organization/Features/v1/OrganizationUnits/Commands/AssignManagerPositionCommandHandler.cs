using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Positions;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class AssignManagerPositionCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<AssignManagerPositionCommand, string>
{
    public async ValueTask<string> Handle(
        AssignManagerPositionCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        Position? managerPosition = null;
        if (!string.IsNullOrWhiteSpace(request.ManagerPositionRefId))
        {
            managerPosition = await dbContext.Positions
                                  .FirstOrDefaultAsync(p => p.ReferenceId == request.ManagerPositionRefId, cancellationToken)
                                  .ConfigureAwait(false)
                              ?? throw new NotFoundException($"Position '{request.ManagerPositionRefId}' was not found.");
        }

        entity.AssignManagerPosition(
            managerPosition: managerPosition,
            allowExternalManagerPosition: request.AllowExternalManagerPosition);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.ReferenceId;
    }
}