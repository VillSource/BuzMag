namespace Villsource.FSH.Modules.Organization.Features.v1.LegalEntities.Endpoints;

internal sealed record SearchLegalEntitiesRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? CountryCode { get; set; }
}
