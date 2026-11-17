namespace Villsource.FSH.Modules.Organization.Features.v1.CostCenters.Endpoints;

internal sealed record SearchCostCentersRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentRefId { get; set; }
    public string? FunctionType { get; set; }
    public string? ResponsibleRefId { get; set; }
    public string? OrgUnitRefId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPostingBlocked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}
