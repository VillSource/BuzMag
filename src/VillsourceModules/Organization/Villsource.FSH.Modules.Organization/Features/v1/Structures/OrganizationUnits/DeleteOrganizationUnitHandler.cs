using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class DeleteOrganizationUnitHandler(
    OrganizationDbContext dbContext) : ICommandHandler<DeleteOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(DeleteOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ou = await dbContext.OrganizationUnits
                     .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                     .ConfigureAwait(false) ??
                 throw new NotFoundException($"Organization Unit with id '{command.Id}' not found.");
        
        ou.Delete();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ou.ToDto();
    }
}