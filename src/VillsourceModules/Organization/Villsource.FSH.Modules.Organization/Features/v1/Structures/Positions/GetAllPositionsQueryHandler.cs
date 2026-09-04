using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class GetAllPositionsQueryHandler(
    OrganizationDbContext dbContext) : IQueryHandler<GetAllPositionsQuery, ICollection<PositionDto>>
{
    public async ValueTask<ICollection<PositionDto>> Handle(GetAllPositionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await dbContext.Positions
            .Select(p => p.ToDto())
            .ToListAsync(cancellationToken);
    }
}