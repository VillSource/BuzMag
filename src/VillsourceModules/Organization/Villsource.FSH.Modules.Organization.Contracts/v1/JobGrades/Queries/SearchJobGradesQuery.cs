using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Queries;

public sealed record SearchJobGradesQuery : IPagedQuery, IRequest<PagedResponse<JobGradeSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public int? MinRankOrder { get; set; }
    public int? MaxRankOrder { get; set; }
    public bool? IsActive { get; set; }
}