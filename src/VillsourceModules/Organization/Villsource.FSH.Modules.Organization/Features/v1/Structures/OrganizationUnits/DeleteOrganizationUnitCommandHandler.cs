using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;
using Villsource.FSH.Modules.Organization.Services;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class DeleteOrganizationUnitCommandHandler(
    OrganizationDbContext dbContext, IOrganizationUnitService service) : ICommandHandler<DeleteOrganizationUnitCommand, OrganizationUnitDto>
{
    public async ValueTask<OrganizationUnitDto> Handle(DeleteOrganizationUnitCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var targetOu = await dbContext.OrganizationUnits
                           .FirstOrDefaultAsync(x => x.ReferenceId == command.Id, cancellationToken)
                           .ConfigureAwait(false) ??
                       throw new NotFoundException($"Organization Unit with id '{command.Id}' not found.");
        targetOu.Delete();

        var descendants = await service.GetDescendantsAsync(targetOu, cancellationToken).ConfigureAwait(false);
        foreach (var descendant in descendants)
        {
            descendant.Delete();
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return targetOu.ToDto();
    }
}