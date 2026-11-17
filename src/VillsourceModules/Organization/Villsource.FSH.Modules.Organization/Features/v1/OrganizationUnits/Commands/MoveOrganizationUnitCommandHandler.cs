using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<MoveOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        MoveOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(e => e.ReferenceId == request.RefId, cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        OrganizationUnit? newParent = null;
        if (!string.IsNullOrWhiteSpace(request.NewParentRefId))
        {
            newParent = await dbContext.OrganizationUnits
                            .FirstOrDefaultAsync(e => e.ReferenceId == request.NewParentRefId, cancellationToken)
                            .ConfigureAwait(false)
                        ?? throw new NotFoundException($"Target parent OrganizationUnit '{request.NewParentRefId}' was not found.");
        }

        var descendants = await dbContext.OrganizationUnits
            .Where(e => e.HierarchyPath.StartsWith(entity.HierarchyPath) && e.Id != entity.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        entity.MoveTo(newParent: newParent, descendants: descendants);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.ReferenceId;
    }
}