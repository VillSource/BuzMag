using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class AllocatePositionCommandHandler(OrganizationDbContext dbContext)
    : ICommandHandler<AllocatePositionCommand, OrganizationUnitPositionAllocationDto>
{
    private OrganizationUnit _ou = null!;
    private Position _position = null!;

    public async ValueTask<OrganizationUnitPositionAllocationDto> Handle(AllocatePositionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        _ou = await dbContext.OrganizationUnits
                  .SingleOrDefaultAsync(x => x.ReferenceId == command.OrganizationUnitId, cancellationToken)
                  .ConfigureAwait(false) ??
              throw new NotFoundException(
                  $"Organization unit with reference '{command.OrganizationUnitId}' not found.");

        _position = await dbContext.Positions
                        .SingleOrDefaultAsync(x => x.ReferenceId == command.PositionId, cancellationToken)
                        .ConfigureAwait(false) ??
                    throw new NotFoundException($"Position with reference '{command.PositionId}' not found.");
        
        if (await dbContext.OrganizationUnitPositionAllocations
                .AnyAsync(x => x.OrganizationUnitId == _ou.Id && x.PositionId == _position.Id && x.EffectiveTo == null,
                    cancellationToken).ConfigureAwait(false))
            throw new CustomException("This position already has a current allocation for the organization unit.", [],
                HttpStatusCode.Conflict);

        var allocation =
            OrganizationUnitPositionAllocation.Create(_ou.Id, _position.Id, command.HeadCount, DateTimeOffset.UtcNow);

        dbContext.OrganizationUnitPositionAllocations.Add(allocation);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return AllocationMapper.ToDto(allocation, _ou.ReferenceId, _position);
    }
}