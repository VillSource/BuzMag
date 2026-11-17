using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class GetJobLookupsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<GetJobLookupsQuery, IReadOnlyList<JobLookupDto>>
{
    public async ValueTask<IReadOnlyList<JobLookupDto>> Handle(
        GetJobLookupsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = dbContext.Jobs.AsNoTracking();

        if (request.ActiveOnly)
        {
            query = query.Where(j => j.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.JobFamily))
        {
            var family = request.JobFamily.Trim();
            query = query.Where(j => j.JobFamily != null && EF.Functions.ILike(j.JobFamily, family));
        }

        return await query
            .OrderBy(j => j.Code)
            .Select(j => new JobLookupDto(
                j.ReferenceId,
                j.Code,
                j.Name,
                j.InternationalName,
                j.JobFamily,
                j.DefaultJobGrade!.ReferenceId,
                j.DefaultJobGrade!.Code,
                j.DefaultJobGrade!.RankOrder))
            .ToListAsync(cancellationToken);
    }
}