using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Thinktecture;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class UpdatePositionCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<UpdatePositionCommand, PositionDto>
{
    public async ValueTask<PositionDto> Handle(UpdatePositionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var position = await dbContext.Positions
                           .FirstOrDefaultAsync(p => p.ReferenceId == command.PositionId, cancellationToken)
                           .ConfigureAwait(false)
                       ?? throw new NotFoundException($"Position with id '{command.PositionId}' not found.");

        position.Modify(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );

        position.SetTier(command.PositionTier?
            .Select(i => PositionTier.TryGet(i, out var tier) ? tier : PositionTier.None)
            .ToHashSet() ?? []);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return position.ToDto();
    }
}