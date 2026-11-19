using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.Jobs.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

public sealed class SearchJobsQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchJobsQuery, PagedResponse<JobSummaryDto>>
{
    public async ValueTask<PagedResponse<JobSummaryDto>> Handle(
        SearchJobsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await dbContext.Jobs
            .ApplySpecification(new SearchJobsSpec(request))
            .ToPagedResponseAsync(request, cancellationToken);
    }
}