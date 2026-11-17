using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Queries;

public sealed class GetCostCenterByRefIdQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<GetCostCenterByRefIdQuery, CostCenterDetailDto>
{
    public async ValueTask<CostCenterDetailDto> Handle(
        GetCostCenterByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new GetCostCenterByRefIdSpec(refId: request.RefId);

        var entity = await dbContext.CostCenters
                         .ApplySpecification(specification: spec)
                         .AsSplitQuery()
                         .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                     ?? throw new NotFoundException($"Cost center '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}
