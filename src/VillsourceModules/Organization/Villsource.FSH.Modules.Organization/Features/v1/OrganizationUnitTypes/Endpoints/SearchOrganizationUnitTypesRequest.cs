namespace Villsource.FSH.Modules.Organization.Features.v1.OrganizationUnitTypes.Endpoints;

internal sealed record SearchOrganizationUnitTypesRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}
