using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;
using Villsource.FSH.Modules.Organization.Mappers;
using Villsource.FSH.Modules.Organization.Services;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class MoveOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext,
    IOrganizationUnitService ouService) : ICommandHandler<MoveOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(MoveOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.NewParentRef == command.UnitRef)
            throw new CustomException("Organization unit can not be child of itself.", [], HttpStatusCode.Conflict);

        var target = await dbContext.OrganizationUnits
                         .ApplySpecification(OrganizationUnitByRefSpec.Create(command.UnitRef))
                         .AsTracking()
                         .SingleOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false) ??
                     throw new NotFoundException(
                         $"Organization unit with id '{command.UnitRef}' not found.");

        var descendants = await ouService.GetDescendantsAsync(target, cancellationToken).ConfigureAwait(false);
        if (descendants.Any(x => x.ReferenceId == command.NewParentRef))
            throw new CustomException("Circular parent detected.", [], HttpStatusCode.Conflict);

        OrganizationUnit? newParent = null;

        if (command.NewParentRef is not null)
        {
            newParent = await dbContext.OrganizationUnits
                            .ApplySpecification(OrganizationUnitByRefSpec.Create(command.NewParentRef))
                            .FirstOrDefaultAsync(cancellationToken)
                            .ConfigureAwait(false) ??
                        throw new NotFoundException(
                            $"Parent organization unit with id '{command.NewParentRef}' not found.");
        }

        target.MoveTo(newParent);
        target.RebaseDescendants(descendants);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return target.ToDto();
    }
}