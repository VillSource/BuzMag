using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<MoveOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(MoveOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ParentId.HasValue == command.OrganizationId.HasValue)
            throw new CustomException("Parent or Organization id must be specified.", [], HttpStatusCode.Conflict);
        
        if (command.ParentId == command.OrganizationUnitId)
            throw new CustomException("Organization unit can not be child of itself.", [], HttpStatusCode.Conflict);

        var target = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(ou => ou.Id == command.OrganizationUnitId, cancellationToken)
                         .ConfigureAwait(false) ??
                     throw new NotFoundException(
                         $"Organization unit with id '{command.OrganizationUnitId}' not found.");
        
        var descendants = await target.GetDescendantsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        if (descendants.Any(x => x.Id == command.ParentId))
            throw new CustomException("Circular parent detected.", [], HttpStatusCode.Conflict);
        
        OrganizationUnit? newParent = null;
        
        if (command.ParentId is not null)
        {
            newParent = await dbContext.OrganizationUnits
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ou => ou.Id == command.ParentId, cancellationToken)
                            .ConfigureAwait(false) ??
                        throw new NotFoundException(
                            $"Parent organization unit with id '{command.ParentId}' not found.");
        }
        
        var orgId = newParent?.OrganizationId ?? command.OrganizationId;
        var newOrganization = await dbContext.Organizations
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(o => orgId == o.Id, cancellationToken)
                                  .ConfigureAwait(false) ??
                              throw new NotFoundException(
                                  $"Organization with id '{command.OrganizationId}' not found.");

        target.Move(newOrganization, newParent);
        target.MoveChildren(descendants);
        
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return target.ToDto();
    }
}