using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class ChangePositionAllocationCommandHandler(OrganizationDbContext dbContext)
    : ICommandHandler<ChangePositionAllocationCommand, PositionAllocationDto>
{
    public async ValueTask<PositionAllocationDto> Handle(ChangePositionAllocationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var current =
            await dbContext.OrganizationUnitPositionAllocations.Include(x => x.Unit).Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Id == command.AllocationId && x.EffectiveTo == null, cancellationToken)
                .ConfigureAwait(false) ?? throw new NotFoundException("Current allocation not found.");
        current.EndEffective();
        
        var replacement = OrganizationUnitPositionAllocation.Create(current.OrganizationUnitId, current.PositionId,
            command.HeadCount, DateTimeOffset.UtcNow);
        
        dbContext.OrganizationUnitPositionAllocations.Add(replacement);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return AllocationMapper.ToDto(replacement, current.Unit.ReferenceId, current.Position);
    }
}