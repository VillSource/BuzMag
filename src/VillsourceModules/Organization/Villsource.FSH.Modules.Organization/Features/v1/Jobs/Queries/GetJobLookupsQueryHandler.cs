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

        if (!string.IsNullOrWhiteSpace(request.JobFamilyRefId))
        {
            var familyRefId = request.JobFamilyRefId.Trim();
            query = query.Where(j => j.JobFamily != null && j.JobFamily.ReferenceId == familyRefId);
        }

        return await query
            .OrderBy(j => j.Code)
            .Select(j => new JobLookupDto(
                j.ReferenceId,
                j.Code,
                j.Name,
                j.InternationalName,
                j.JobFamily != null ? j.JobFamily.Name : null,
                j.DefaultJobGrade!.ReferenceId,
                j.DefaultJobGrade!.Code,
                j.DefaultJobGrade!.RankOrder))
            .ToListAsync(cancellationToken);
    }
}