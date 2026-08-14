using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class CreatePositionCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<CreatePositionCommand, PositionDto>
{
    public async ValueTask<PositionDto> Handle(CreatePositionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var organization = string.IsNullOrWhiteSpace(command.OrganizationId)
            ? await dbContext.Organizations
                .FirstOrDefaultAsync(o => o.IsDefault, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException("Default Organization not found.")
            : await dbContext.Organizations
                .FirstOrDefaultAsync(o => o.ReferenceId == command.OrganizationId, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException($"Organization with id '{command.OrganizationId}' not found.");

        var position = Position.Create(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );

        organization.Positions.Add(position);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return position.ToDto();
    }
}
