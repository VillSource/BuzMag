using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.PositionAllocations;

public sealed class GetPositionAllocationHistoryQueryHandler(OrganizationDbContext dbContext)
    : IQueryHandler<GetPositionAllocationHistoryQuery, ICollection<PositionAllocationDto>>
{
    public async ValueTask<ICollection<PositionAllocationDto>> Handle(
        GetPositionAllocationHistoryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var unit = await dbContext.OrganizationUnits.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReferenceId == query.OrganizationUnitId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Organization unit not found.");
        
        var allocations = await dbContext.OrganizationUnitPositionAllocations.AsNoTracking()
            .Include(x => x.Position)
            .Where(x => x.OrganizationUnitId == unit.Id && x.Position.ReferenceId == query.PositionId)
            .OrderByDescending(x => x.EffectiveFrom).ToListAsync(cancellationToken).ConfigureAwait(false);
        
        return allocations.Select(x => AllocationMapper.ToDto(x, unit.ReferenceId)).ToList();
    }
}