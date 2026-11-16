using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Queries;

public sealed record SearchJobsQuery : IPagedQuery, IRequest<PagedResponse<JobSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? JobFamily { get; set; }
    public string? DefaultJobGradeRefId { get; set; }
    public bool? IsActive { get; set; }
}