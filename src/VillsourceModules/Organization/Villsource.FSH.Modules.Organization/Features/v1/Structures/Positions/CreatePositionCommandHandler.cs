using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
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
        
        string normalizeCode = command.Code.ToUpperInvariant();
        if (await dbContext.Positions
                .AnyAsync(i => i.Code == normalizeCode, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new CustomException($"Position with code '{command.Code}' already exists", [], statusCode: HttpStatusCode.Conflict);
        }

        var position = Position.Create(
            code: command.Code,
            name: command.Name,
            description: command.Description
        );

        foreach (var tier in command.PositionTier ?? [])  
        {
            if (PositionTier.TryGet(tier, out var positionTier))
                position.PositionTiers.Add(positionTier);
        }

        dbContext.Positions.Add(position);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return position.ToDto();
    }
}
