using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobFamilies.Queries;

public sealed record SearchJobFamiliesQuery : IPagedQuery, IRequest<PagedResponse<JobFamilySummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}
