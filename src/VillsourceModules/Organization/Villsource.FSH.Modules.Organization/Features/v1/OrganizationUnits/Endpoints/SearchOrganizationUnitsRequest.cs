namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnits.Endpoints;

internal sealed record SearchOrganizationUnitsRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? ParentRefId { get; set; }
    public string? ManagerRefId { get; set; }
    public string? TypeRefId { get; set; }
    public bool? IsActive { get; set; }
    public bool RootOnly { get; set; }
    public bool VacantManagerOnly { get; set; }
}

internal sealed record GetOrganizationTreeRequest(
    string LegalEntityRefId,
    int? MaxDepth = null,
    bool? ActiveOnly = null);
