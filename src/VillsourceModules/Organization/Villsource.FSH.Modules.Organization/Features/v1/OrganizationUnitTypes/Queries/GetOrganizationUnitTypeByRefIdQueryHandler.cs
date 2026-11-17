using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.OrganizationUnitTypes.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Queries;

public sealed class GetOrganizationUnitTypeByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetOrganizationUnitTypeByRefIdQuery, OrganizationUnitTypeDetailDto>
{
    public async ValueTask<OrganizationUnitTypeDetailDto> Handle(
        GetOrganizationUnitTypeByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.OrganizationUnitTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Organization unit type '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}
