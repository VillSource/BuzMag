namespace Villsource.FSH.Modules.Organization.Features.v1.PositionDelegations.Endpoints;

internal sealed record SearchPositionDelegationsRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
    public string? LegalEntityRefId { get; set; }
    public string? DelegatorRefId { get; set; }
    public string? DelegateRefId { get; set; }
    public bool? IsRevoked { get; set; }
    public DateTimeOffset? EffectiveAtUtc { get; set; }
}

internal sealed record ResolveEffectiveDelegationsRequest(
    string DelegatorRefId,
    string? DocTypeCode = null,
    DateTimeOffset? AtUtc = null);
