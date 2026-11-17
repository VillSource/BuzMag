namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades.Endpoints;

internal sealed record SearchJobGradesRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? Keyword { get; set; }
    public int? MinRank { get; set; }
    public int? MaxRank { get; set; }
    public bool? IsActive { get; set; }
}
