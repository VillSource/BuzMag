using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class UpdateOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<UpdateOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(UpdateOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var targetOu = await dbContext.OrganizationUnits
                     .FirstOrDefaultAsync(ou => ou.Id == command.OrganizationUnitId, cancellationToken)
                     .ConfigureAwait(false) ??
                 throw new NotFoundException($"Organization unit with id '{command.OrganizationUnitId}' not found.");

        targetOu.Update(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return targetOu.ToDto();
    }
}