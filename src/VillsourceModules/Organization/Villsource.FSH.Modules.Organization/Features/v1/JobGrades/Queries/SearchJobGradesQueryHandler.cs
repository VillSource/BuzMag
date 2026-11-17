using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Queries
{

    public sealed class SearchJobGradesQueryHandler(OrganizationDbContext dbContext)
        : IRequestHandler<SearchJobGradesQuery, PagedResponse<JobGradeSummaryDto>>
    {
        public async ValueTask<PagedResponse<JobGradeSummaryDto>> Handle(
            SearchJobGradesQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await dbContext.JobGrades
                .ApplySpecification(new SearchJobGradesSpec(request))
                .ToPagedResponseAsync(request, cancellationToken);
        }
    }

}