using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;
using Villsource.FSH.Modules.Organization.Services;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext,
    IOrganizationUnitService service) : ICommandHandler<MoveOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(MoveOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.ParentId) == string.IsNullOrWhiteSpace(command.OrganizationId))
            throw new CustomException("Parent or Organization id must be specified.", [], HttpStatusCode.Conflict);

        if (command.ParentId == command.OrganizationUnitId)
            throw new CustomException("Organization unit can not be child of itself.", [], HttpStatusCode.Conflict);

        var target = await dbContext.OrganizationUnits
                         .FirstOrDefaultAsync(ou => ou.ReferenceId == command.OrganizationUnitId, cancellationToken)
                         .ConfigureAwait(false) ??
                     throw new NotFoundException(
                         $"Organization unit with id '{command.OrganizationUnitId}' not found.");

        var descendants = await service.GetDescendantsAsync(target, cancellationToken).ConfigureAwait(false);
        if (descendants.Any(x => x.ReferenceId == command.ParentId))
            throw new CustomException("Circular parent detected.", [], HttpStatusCode.Conflict);

        OrganizationUnit? newParent = null;

        if (command.ParentId is not null)
        {
            newParent = await dbContext.OrganizationUnits
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ou => ou.ReferenceId == command.ParentId, cancellationToken)
                            .ConfigureAwait(false) ??
                        throw new NotFoundException(
                            $"Parent organization unit with id '{command.ParentId}' not found.");
        }

        var newOrganization = await dbContext.Organizations
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(o => newParent!.OrganizationId == o.Id || command.OrganizationId == o.ReferenceId, cancellationToken)
                                  .ConfigureAwait(false) ??
                              throw new NotFoundException(
                                  $"Organization with id '{command.OrganizationId}' not found.");

        target.MoveTo(newOrganization, newParent);
        target.RebaseDescendants(descendants);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return target.ToDto();
    }
}