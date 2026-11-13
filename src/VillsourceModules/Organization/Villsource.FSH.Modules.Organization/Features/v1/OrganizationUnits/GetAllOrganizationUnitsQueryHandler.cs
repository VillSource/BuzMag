using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits;

public sealed class GetAllOrganizationUnitsQueryHandler(
    OrganizationDbContext dbContext) : IQueryHandler<GetAllOrganizationUnitsQuery, ICollection<OrganizationUnitDto>>
{
    public async ValueTask<ICollection<OrganizationUnitDto>> Handle(GetAllOrganizationUnitsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var units = await dbContext.OrganizationUnits
            .AsNoTracking()
            .Select(ou => ou.ToDto())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return OrganizationUnitDto.BuildTree(units);
    }
}