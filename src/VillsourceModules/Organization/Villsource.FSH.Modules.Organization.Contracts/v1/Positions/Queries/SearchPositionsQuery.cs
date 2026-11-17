using FSH.Framework.Shared.Persistence;
using Mediator;
using Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Queries;

public sealed record SearchPositionsQuery : IPagedQuery, IRequest<PagedResponse<PositionSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? OrgUnitRefId { get; set; }
    public string? JobRefId { get; set; }
    public string? JobGradeRefId { get; set; }
    public string? CostCenterRefId { get; set; }
    public string? ReportsToPositionRefId { get; set; }
    public bool? IsPrimaryLeadership { get; set; }
    public bool? IsActive { get; set; }
}