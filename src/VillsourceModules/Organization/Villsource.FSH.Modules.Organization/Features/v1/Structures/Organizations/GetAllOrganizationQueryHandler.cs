using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Structures;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Mappers;

namespace Villsource.FSH.Modules.Organization.Features.v1.Structures.Organizations;

public sealed class GetAllOrganizationQueryHandler(
    OrganizationDbContext dbContext) : IQueryHandler<GetAllOrganizationsQuery, ICollection<OrganizationDto>>
{
    public async ValueTask<ICollection<OrganizationDto>> Handle(GetAllOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await dbContext.Organizations
            .AsNoTracking()
            .Select(org => org.ToDto())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}