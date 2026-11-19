using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnits.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetOrganizationUnitByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetOrganizationUnitByRefIdQuery, OrganizationUnitDetailDto>
{
    public async ValueTask<OrganizationUnitDetailDto> Handle(
        GetOrganizationUnitByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new GetOrganizationUnitByRefIdSpec(refId: request.RefId);
        var entity = await dbContext.OrganizationUnits
                         .ApplySpecification(specification: spec)
                         .AsSplitQuery()
                         .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new NotFoundException($"OrganizationUnit '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}