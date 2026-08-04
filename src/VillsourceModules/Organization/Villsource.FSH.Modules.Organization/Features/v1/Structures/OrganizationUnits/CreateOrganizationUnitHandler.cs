using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class CreateOrganizationUnitHandler(
    OrganizationDbContext dbContext) : ICommandHandler<CreateOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parent = command.ParentId is not null
            ? await dbContext
                .OrganizationUnits
                .FirstOrDefaultAsync(ou => ou.Id == command.ParentId, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException($"Parent with id '{command.ParentId}' not found.")
            : null;

        var org = parent is null
            ? await dbContext.Organizations
                .FirstOrDefaultAsync(x => x.IsDefault, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException($"Organization not found.")
            : null;

        var unit = parent is null
            ? OrganizationUnit.Create(
                code: command.Code,
                name: command.Name,
                description: command.Description,
                organization: org
            )
            : parent.CreateChild(
                code: command.Code,
                name: command.Name,
                description: command.Description
            );

        dbContext.OrganizationUnits.Add(unit);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return unit.ToDto();
    }
}