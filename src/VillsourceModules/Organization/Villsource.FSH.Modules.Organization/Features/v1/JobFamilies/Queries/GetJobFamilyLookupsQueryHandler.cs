using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Queries;

public sealed class GetJobFamilyLookupsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetJobFamilyLookupsQuery, IReadOnlyList<JobFamilyLookupDto>>
{
    public async ValueTask<IReadOnlyList<JobFamilyLookupDto>> Handle(
        GetJobFamilyLookupsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = dbContext.JobFamilies.AsNoTracking();
        if (request.ActiveOnly)
        {
            query = query.Where(f => f.IsActive);
        }

        return await query
            .OrderBy(f => f.Code)
            .Select(f => new JobFamilyLookupDto(
                f.ReferenceId,
                f.Code,
                f.Name,
                f.InternationalName))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
