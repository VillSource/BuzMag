namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Endpoints;

internal sealed record SearchPositionsRequest
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
    public string? ReportsToRefId { get; set; }
    public bool? IsLeadership { get; set; }
    public bool? IsActive { get; set; }
}

internal sealed record GetPositionLookupsRequest(
    string? LegalEntityRefId = null,
    string? OrgUnitRefId = null,
    bool ActiveOnly = true,
    bool LeadershipOnly = false);

internal sealed record GetPositionReportingChainRequest(
    int MaxDepth = 15,
    bool IncludeInactive = false);

internal sealed record GetVacancyStatusRequest(
    string LegalEntityRefId,
    string? OrgUnitRefId = null,
    bool ActiveOnly = true);

internal sealed record ResolveApprovalAuthorityRequest(
    string? DocTypeCode = null,
    DateTimeOffset? AsOf = null);

internal sealed record GetPositionSubtreeRequest(
    int MaxDepth = 10,
    bool IncludeInactive = false);
