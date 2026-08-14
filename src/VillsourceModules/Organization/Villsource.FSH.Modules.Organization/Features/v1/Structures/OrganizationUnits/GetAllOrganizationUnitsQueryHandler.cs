using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.OrganizationUnits;

public sealed class GetAllOrganizationUnitsQueryHandler(
    OrganizationDbContext dbContext) : IQueryHandler<GetAllOrganizationUnitsQuery, ICollection<OrganizationUnitDto>>
{
    public async ValueTask<ICollection<OrganizationUnitDto>> Handle(GetAllOrganizationUnitsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var organizationQuery = string.IsNullOrWhiteSpace(query.OrganizationId) 
            ? dbContext.Organizations
                .AsNoTracking()
                .Include(o => o.Units)
                .FirstOrDefaultAsync(o => o.IsDefault, cancellationToken)
            : dbContext.Organizations
                .AsNoTracking()
                .Include(o => o.Units)
                .FirstOrDefaultAsync(o => o.ReferenceId == query.OrganizationId, cancellationToken);
        
        var organization = await organizationQuery
                               .ConfigureAwait(false)
                           ?? throw new NotFoundException("Organization not found.");
        
        var units = organization.Units.Select(ou => ou.ToDto()).ToList();
        var unitTree = OrganizationUnitDto.BuildTree(units);

        return unitTree;
    }
}
