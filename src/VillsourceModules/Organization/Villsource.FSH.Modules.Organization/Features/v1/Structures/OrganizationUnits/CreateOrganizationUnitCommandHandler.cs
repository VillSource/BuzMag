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

public sealed class CreateOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<CreateOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);


        if (command.ParentId is null)
        {
            var topOrganizationUnit = OrganizationUnit.Create(
                code: command.Code,
                name: command.Name,
                description: command.Description
            );
            dbContext.OrganizationUnits.Add(topOrganizationUnit);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return topOrganizationUnit.ToDto();
        }

        var parentOu = await dbContext.OrganizationUnits
                           .FirstOrDefaultAsync(x => x.ReferenceId == command.ParentId, cancellationToken)
                           .ConfigureAwait(false)
                       ?? throw new NotFoundException("Parent OU not found.");

        var childOrganizationUnit = parentOu.CreateChild(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return childOrganizationUnit.ToDto();
    }
}