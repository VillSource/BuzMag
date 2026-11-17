using FSH.Framework.Shared.Persistence;
using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.CostCenters.Queries;

public sealed record SearchCostCentersQuery : IPagedQuery, IRequest<PagedResponse<CostCenterSummaryDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentCostCenterRefId { get; set; }
    public string? FunctionType { get; set; }
    public string? ResponsiblePositionRefId { get; set; }
    public string? OrgUnitRefId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPostingBlocked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}