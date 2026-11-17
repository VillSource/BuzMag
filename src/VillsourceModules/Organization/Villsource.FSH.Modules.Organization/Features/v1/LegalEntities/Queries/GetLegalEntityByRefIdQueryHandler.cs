using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.LegalEntities.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities;

public sealed class GetLegalEntityByRefIdQueryHandler(
    OrganizationDbContext dbContext)
    : IRequestHandler<GetLegalEntityByRefIdQuery, LegalEntityDetailDto>
{
    public async ValueTask<LegalEntityDetailDto> Handle(
        GetLegalEntityByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var spec = new GetLegalEntityByRefIdSpec(refId: request.RefId);

        var entity = await dbContext.LegalEntities
                         .ApplySpecification(specification: spec)
                         .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                     ?? throw new NotFoundException($"Legal entity '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}