using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class DeletePositionCommandHandler(
    OrganizationDbContext dbContext) : ICommandHandler<DeletePositionCommand, PositionDto>
{
    public async ValueTask<PositionDto> Handle(DeletePositionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var position = await dbContext.Positions
            .FirstOrDefaultAsync(p => p.ReferenceId == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Position with id '{command.Id}' not found.");

        position.Delete();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return position.ToDto();
    }
}
