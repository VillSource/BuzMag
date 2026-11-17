namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs.Endpoints;

internal sealed record SearchJobsRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public string? FamilyRefId { get; set; }
    public string? JobGradeRefId { get; set; }
    public bool? IsActive { get; set; }
}

internal sealed record GetJobLookupsRequest(
    bool ActiveOnly = true,
    string? FamilyRefId = null);
