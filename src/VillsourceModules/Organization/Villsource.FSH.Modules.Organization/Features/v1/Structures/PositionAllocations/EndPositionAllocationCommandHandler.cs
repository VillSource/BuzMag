using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures.PositionAllocations;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class EndPositionAllocationCommandHandler(OrganizationDbContext dbContext)
    : ICommandHandler<EndPositionAllocationCommand, OrganizationUnitPositionAllocationDto>
{
    public async ValueTask<OrganizationUnitPositionAllocationDto> Handle(EndPositionAllocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var allocation =
            await dbContext.OrganizationUnitPositionAllocations.Include(x => x.Unit).Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Id == command.AllocationId && x.EffectiveTo == null, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException("Current allocation not found.");
        allocation.EndEffective();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return AllocationMapper.ToDto(allocation, allocation.Unit.ReferenceId);
    }
}