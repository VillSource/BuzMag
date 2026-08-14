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

        OrganizationUnit organizationUnit;

        if (command.ParentId is null)
        {
            // Add to root of Organization.
            var organization = command.OrganizationId is null
                ? await GetDefaultOrganization(cancellationToken)
                    .ConfigureAwait(false) ?? throw new NotFoundException("Default Organization not found.")
                : await dbContext.Organizations
                      .FirstOrDefaultAsync(x => x.ReferenceId == command.OrganizationId, cancellationToken)
                      .ConfigureAwait(false) ??
                  throw new NotFoundException($"Organization with id '{command.OrganizationId}' not found.");

            organizationUnit = OrganizationUnit.Create(
                code: command.Code,
                name: command.Name,
                description: command.Description
            );
            organization.Units.Add(organizationUnit);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return organizationUnit.ToDto();
        }
        
        if (command.OrganizationId is null)
        {
            // Add to parent organization unit
            var parentOu = await dbContext.OrganizationUnits
                               .FirstOrDefaultAsync(x => x.ReferenceId == command.ParentId, cancellationToken)
                               .ConfigureAwait(false)
                           ?? throw new NotFoundException("Parent OU not found.");

            organizationUnit = parentOu.CreateChild(
                code: command.Code,
                name: command.Name,
                description: command.Description
            );
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return organizationUnit.ToDto();
        }

        throw new CustomException("Only one 'ParentId' or 'OrganizationId' can be specified.", [],
            HttpStatusCode.Conflict);
    }

    private Task<Domain.Organization?> GetDefaultOrganization(CancellationToken ct = default)
    {
        return dbContext.Organizations
            .FirstOrDefaultAsync(x => x.IsDefault, ct);
    }
}