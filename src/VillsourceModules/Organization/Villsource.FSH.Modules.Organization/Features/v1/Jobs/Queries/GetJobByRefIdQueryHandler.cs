using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Jobs.Specifications;


namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class GetJobByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetJobByRefIdQuery, JobDetailDto>
{
    public async ValueTask<JobDetailDto> Handle(
        GetJobByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.Jobs
                         .ApplySpecification(new GetJobByRefIdSpec(request.RefId))
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Job '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}