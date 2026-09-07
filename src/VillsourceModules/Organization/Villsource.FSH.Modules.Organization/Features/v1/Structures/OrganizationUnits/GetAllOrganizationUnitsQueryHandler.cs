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

        var units = await dbContext.OrganizationUnits
            .Select(ou => ou.ToDto())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return OrganizationUnitDto.BuildTree(units);
    }
}