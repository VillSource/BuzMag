using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Queries;

public sealed class GetPositionDelegationByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetPositionDelegationByRefIdQuery, PositionDelegationDetailDto>
{
    public async ValueTask<PositionDelegationDetailDto> Handle(
        GetPositionDelegationByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.PositionDelegations
                         .ApplySpecification(new GetPositionDelegationByRefIdSpec(request.RefId))
                         .AsSplitQuery()
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Position delegation '{request.RefId}' was not found.");

        return Mappers.PositionDelegationMapper.ToDetailDto(entity);
    }
}