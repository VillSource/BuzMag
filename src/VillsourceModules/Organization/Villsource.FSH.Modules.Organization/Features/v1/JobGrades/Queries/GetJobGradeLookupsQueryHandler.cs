using Mediator;
using Microsoft.EntityFrameworkCore;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Data;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries
{
    public sealed class GetJobGradeLookupsQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<GetJobGradeLookupsQuery, IReadOnlyList<JobGradeLookupDto>>
    {
        public async ValueTask<IReadOnlyList<JobGradeLookupDto>> Handle(
            GetJobGradeLookupsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = dbContext.JobGrades.AsNoTracking();

            if (request.ActiveOnly)
            {
                query = query.Where(g => g.IsActive);
            }

            return await query
                .OrderBy(g => g.RankOrder)
                .ThenBy(g => g.Code)
                .Select(g => new JobGradeLookupDto(
                    g.ReferenceId,
                    g.Code,
                    g.Name,
                    g.InternationalName,
                    g.RankOrder))
                .ToListAsync(cancellationToken);
        }
    }

}