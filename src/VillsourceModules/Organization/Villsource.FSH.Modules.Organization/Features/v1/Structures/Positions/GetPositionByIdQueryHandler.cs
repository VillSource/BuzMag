using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Positions;

public sealed class GetPositionByIdQueryHandler(
    OrganizationDbContext dbContext) : IQueryHandler<GetPositionByIdQuery, PositionDto>
{
    public async ValueTask<PositionDto> Handle(GetPositionByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var position = await dbContext.Positions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ReferenceId == query.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Position with id '{query.Id}' not found.");

        return position.ToDto();
    }
}
