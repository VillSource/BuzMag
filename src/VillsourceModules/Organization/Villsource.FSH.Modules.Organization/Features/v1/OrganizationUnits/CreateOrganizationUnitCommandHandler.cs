using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class CreateOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<CreateOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ParentRef is null)
        {
            var topLevelOu = OrganizationUnit.Create(
                code: command.Code,
                name: command.Name,
                description: command.Description
            );
            dbContext.OrganizationUnits.Add(topLevelOu);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return topLevelOu.ToDto();
        }

        var parentOu = await dbContext.OrganizationUnits
                           .ApplySpecification(OrganizationUnitByRefSpec.Create(command.ParentRef))
                           .AsTracking()
                           .SingleOrDefaultAsync(cancellationToken)
                           .ConfigureAwait(false)
                       ?? throw new NotFoundException("Parent OU not found.");

        var childOu = parentOu.CreateChild(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return childOu.ToDto();
    }
}