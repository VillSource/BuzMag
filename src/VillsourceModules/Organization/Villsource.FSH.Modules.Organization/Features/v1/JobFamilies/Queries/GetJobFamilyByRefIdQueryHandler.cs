using FSH.Framework.Core.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Exceptions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Queries;

public sealed class GetJobFamilyByRefIdQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetJobFamilyByRefIdQuery, JobFamilyDetailDto>
{
    public async ValueTask<JobFamilyDetailDto> Handle(
        GetJobFamilyByRefIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.JobFamilies
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.ReferenceId == request.RefId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Job family '{request.RefId}' was not found.");

        return entity.ToDetailDto();
    }
}
