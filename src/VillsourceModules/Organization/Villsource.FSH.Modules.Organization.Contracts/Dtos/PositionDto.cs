using System.Text.Json.Serialization;
using Villsource.FSH.Modules.Organization.Contracts.Constants;

namespace Villsource.FSH.Modules.Organization.Contracts.Dtos;

public sealed class PositionDto
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public IReadOnlyCollection<PositionTier> PositionTiers { get;  set; } = [];
    public string? Description { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
}