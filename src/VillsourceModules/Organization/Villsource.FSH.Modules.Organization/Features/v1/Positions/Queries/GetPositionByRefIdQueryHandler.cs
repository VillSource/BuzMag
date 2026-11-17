using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Positions.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Queries;

public sealed class GetPositionByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetPositionByRefIdQuery, PositionDetailDto>
{
    public async ValueTask<PositionDetailDto> Handle(
        GetPositionByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.Positions
                         .ApplySpecification(new GetPositionByRefIdSpec(request.RefId))
                         .AsSplitQuery()
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Position '{request.RefId}' was not found.");

        return Mappers.PositionMapper.ToDetailDto(entity);
    }
}