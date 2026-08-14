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

        var organizationQuery = string.IsNullOrWhiteSpace(query.OrganizationId)
            ? dbContext.Organizations
                .AsNoTracking()
                .Include(o => o.Positions)
                .FirstOrDefaultAsync(o => o.IsDefault, cancellationToken)
            : dbContext.Organizations
                .AsNoTracking()
                .Include(o => o.Positions)
                .FirstOrDefaultAsync(o => o.ReferenceId == query.OrganizationId, cancellationToken);

        var organization = await organizationQuery
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Organization not found.");

        return organization.Positions.Select(p => p.ToDto()).ToList();
    }
}
