using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Specifications;

namespace Villsource.FSH.Modules.Organization.Features.v1.JobFamilies.Queries;

public sealed class SearchJobFamiliesQueryHandler(OrganizationDbContext dbContext)
    : IRequestHandler<SearchJobFamiliesQuery, PagedResponse<JobFamilySummaryDto>>
{
    public async ValueTask<PagedResponse<JobFamilySummaryDto>> Handle(
        SearchJobFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await dbContext.JobFamilies
            .ApplySpecification(new SearchJobFamiliesSpec(request))
            .ToPagedResponseAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }
}
