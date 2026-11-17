using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Commands;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class DeactivateOrganizationUnitCommandHandler(OrganizationDbContext dbContext)
    : IRequestHandler<DeactivateOrganizationUnitCommand, string>
{
    public async ValueTask<string> Handle(
        DeactivateOrganizationUnitCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var target = await dbContext.OrganizationUnits
                         .Where(e => e.ReferenceId == request.RefId)
                         .Select(e => new
                         {
                             Entity = e,
                             HasActiveChildren = dbContext.OrganizationUnits.Any(c =>
                                 !c.IsDeleted && c.IsActive && c.HierarchyPath.StartsWith(e.HierarchyPath) && c.Id != e.Id),
                             HasActivePositions = dbContext.Positions.Any(p =>
                                 !p.IsDeleted && p.IsActive && p.OrgUnitId == e.Id)
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        if (target.HasActiveChildren)
        {
            throw new InvalidOperationException(
                $"Cannot deactivate OrganizationUnit '{request.RefId}' while active child organization units still exist.");
        }

        if (target.HasActivePositions)
        {
            throw new InvalidOperationException(
                $"Cannot deactivate OrganizationUnit '{request.RefId}' while active positions still belong to it.");
        }

        target.Entity.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return target.Entity.ReferenceId;
    }
}